using System.Collections.ObjectModel;
using System.Windows.Input;
using JournalDaily.Models;
using JournalDaily.Services;

namespace JournalDaily.ViewModels
{
    /// <summary>
    /// MVVM ViewModel for PDF export functionality.
    /// Manages date range selection, PDF generation, and export history.
    /// </summary>
    public class PdfExportViewModel : BaseViewModel
    {
        private readonly PdfExportService _pdfExportService;
        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        private DateTime _endDate = DateTime.Now;
        private string _statusMessage = string.Empty;
        private bool _isLoading = false;
        private bool _showSuccess = false;
        private PdfExportResult? _lastExportResult;
        private ObservableCollection<ExportedFileInfo> _exportedFiles;

        public PdfExportViewModel(PdfExportService pdfExportService)
        {
            _pdfExportService = pdfExportService;
            _exportedFiles = new ObservableCollection<ExportedFileInfo>();

            // Initialize commands
            ExportCommand = new Command(OnExport, CanExport);
            ClearStatusCommand = new Command(OnClearStatus);
            OpenPdfCommand = new Command<ExportedFileInfo>(OnOpenPdf);
            DeleteFileCommand = new Command<ExportedFileInfo>(OnDeleteFile);
            RefreshFilesCommand = new Command(OnRefreshFiles);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    ((Command)ExportCommand).ChangeCanExecute();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    ((Command)ExportCommand).ChangeCanExecute();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ShowSuccess
        {
            get => _showSuccess;
            set => SetProperty(ref _showSuccess, value);
        }

        public PdfExportResult? LastExportResult
        {
            get => _lastExportResult;
            set => SetProperty(ref _lastExportResult, value);
        }

        public ObservableCollection<ExportedFileInfo> ExportedFiles
        {
            get => _exportedFiles;
            set => SetProperty(ref _exportedFiles, value);
        }

        /// <summary>
        /// Validation info for date range.
        /// </summary>
        public string DateRangeInfo
        {
            get
            {
                if (EndDate < StartDate)
                {
                    return "End date must be after start date";
                }

                int daysDifference = (EndDate - StartDate).Days + 1;
                return $"{daysDifference} {(daysDifference == 1 ? "day" : "days")}";
            }
        }

        public ICommand ExportCommand { get; }
        public ICommand ClearStatusCommand { get; }
        public ICommand OpenPdfCommand { get; }
        public ICommand DeleteFileCommand { get; }
        public ICommand RefreshFilesCommand { get; }

        private bool CanExport()
        {
            return EndDate >= StartDate && !IsLoading;
        }

        private async void OnExport()
        {
            if (!CanExport())
            {
                return;
            }

            IsLoading = true;
            ShowSuccess = false;
            StatusMessage = "Generating PDF...";

            try
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Exporting entries from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");

                var result = await _pdfExportService.ExportEntriesAsync(StartDate, EndDate);

                if (result.IsSuccess)
                {
                    LastExportResult = result;
                    StatusMessage = $"✅ Export successful! {result.EntriesCount} {(result.EntriesCount == 1 ? "entry" : "entries")} exported.";
                    ShowSuccess = true;

                    System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Export successful: {result.FileName}");

                    // Refresh file list
                    await Task.Delay(500); // Brief delay for UX
                    OnRefreshFiles();
                }
                else
                {
                    StatusMessage = $"❌ Export failed: {result.ErrorMessage}";
                    ShowSuccess = false;

                    System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Export failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Error during export: {ex.Message}");
                StatusMessage = $"❌ An error occurred: {ex.Message}";
                ShowSuccess = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnClearStatus()
        {
            StatusMessage = string.Empty;
            ShowSuccess = false;
        }

        private async void OnOpenPdf(ExportedFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return;
            }

            IsLoading = true;
            StatusMessage = "Opening PDF...";

            try
            {
                bool opened = await _pdfExportService.OpenPdfAsync(fileInfo.FullPath);

                if (!opened)
                {
                    StatusMessage = "❌ Could not open PDF. No PDF viewer available.";
                }
                else
                {
                    StatusMessage = "✅ PDF opened successfully.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Error opening PDF: {ex.Message}");
                StatusMessage = $"❌ Error opening PDF: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnDeleteFile(ExportedFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return;
            }

            bool confirmed = await Shell.Current.DisplayAlert(
                "Delete File",
                $"Delete '{fileInfo.FileName}'?",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            try
            {
                bool deleted = _pdfExportService.DeleteExportedFile(fileInfo.FullPath);

                if (deleted)
                {
                    ExportedFiles.Remove(fileInfo);
                    StatusMessage = "✅ File deleted.";
                    System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Deleted file: {fileInfo.FileName}");
                }
                else
                {
                    StatusMessage = "❌ Could not delete file.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Error deleting file: {ex.Message}");
                StatusMessage = $"❌ Error deleting file: {ex.Message}";
            }
        }

        private void OnRefreshFiles()
        {
            try
            {
                var files = _pdfExportService.GetExportedFiles();
                ExportedFiles.Clear();

                foreach (var file in files)
                {
                    ExportedFiles.Add(new ExportedFileInfo
                    {
                        FileName = file.Name,
                        FullPath = file.FullName,
                        CreatedDate = file.CreationTime,
                        SizeBytes = file.Length
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Loaded {ExportedFiles.Count} exported files");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportViewModel] Error refreshing files: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize view model - load exported files.
        /// </summary>
        public void Initialize()
        {
            StatusMessage = string.Empty;
            ShowSuccess = false;
            OnRefreshFiles();

            System.Diagnostics.Debug.WriteLine("[PdfExportViewModel] Initialized");
        }

        /// <summary>
        /// Presets for quick date range selection.
        /// </summary>
        public void SetDatePreset(string preset)
        {
            var today = DateTime.Now;

            switch (preset)
            {
                case "today":
                    StartDate = today;
                    EndDate = today;
                    break;
                case "week":
                    StartDate = today.AddDays(-(int)today.DayOfWeek);
                    EndDate = today;
                    break;
                case "month":
                    StartDate = new DateTime(today.Year, today.Month, 1);
                    EndDate = today;
                    break;
                case "year":
                    StartDate = new DateTime(today.Year, 1, 1);
                    EndDate = today;
                    break;
                case "all":
                    StartDate = DateTime.Now.AddYears(-10); // Assume 10 years max history
                    EndDate = today;
                    break;
            }

            ((Command)ExportCommand).ChangeCanExecute();
        }
    }

    /// <summary>
    /// Model for displaying exported file information.
    /// </summary>
    public class ExportedFileInfo
    {
        public required string FileName { get; set; }
        public required string FullPath { get; set; }
        public DateTime CreatedDate { get; set; }
        public long SizeBytes { get; set; }

        public string CreatedDateDisplay => CreatedDate.ToString("MMM d, yyyy h:mm tt");

        public string FileSizeDisplay
        {
            get
            {
                return SizeBytes switch
                {
                    < 1024 => $"{SizeBytes} B",
                    < 1024 * 1024 => $"{SizeBytes / 1024.0:F1} KB",
                    _ => $"{SizeBytes / (1024.0 * 1024):F1} MB"
                };
            }
        }
    }
}
