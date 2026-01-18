using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font;
using iText.Kernel.Font;
using JournalDaily.Data;
using JournalDaily.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;

namespace JournalDaily.Services
{
    /// <summary>
    /// Service for exporting journal entries to PDF format.
    /// </summary>
    public class PdfExportService
    {
        private readonly AppDbContext _dbContext;
        private const string EXPORTS_FOLDER = "JournalExports";

        public PdfExportService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Exports journal entries from the specified date range to a PDF file.
        /// </summary>
        public async Task<PdfExportResult> ExportEntriesAsync(DateTime startDate, DateTime endDate)
        {
            var result = new PdfExportResult();

            try
            {
                // Normalize dates to start and end of day
                startDate = startDate.Date;
                endDate = endDate.Date.AddDays(1).AddTicks(-1);

                System.Diagnostics.Debug.WriteLine($"[PdfExportService] Exporting entries from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Fetch entries from database
                var entries = await _dbContext.JournalEntries
                    .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
                    .OrderBy(e => e.EntryDate)
                    .ToListAsync();

                if (entries.Count == 0)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "No journal entries found in the selected date range.";
                    return result;
                }

                System.Diagnostics.Debug.WriteLine($"[PdfExportService] Found {entries.Count} entries to export");

                // Create exports directory if it doesn't exist
                string exportsPath = Path.Combine(FileSystem.AppDataDirectory, EXPORTS_FOLDER);
                Directory.CreateDirectory(exportsPath);

                // Generate PDF file path
                string fileName = GenerateFileName(startDate, endDate);
                string filePath = Path.Combine(exportsPath, fileName);

                // Generate PDF
                GeneratePdf(filePath, entries, startDate, endDate);

                // Get file info
                var fileInfo = new FileInfo(filePath);

                result.IsSuccess = true;
                result.FilePath = filePath;
                result.FileName = fileName;
                result.EntriesCount = entries.Count;
                result.FileSizeBytes = fileInfo.Length;
                result.DateRangeDisplay = $"{startDate:MMM dd} - {endDate:MMM dd, yyyy}";

                System.Diagnostics.Debug.WriteLine($"[PdfExportService] PDF created successfully: {filePath}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportService] Error exporting entries: {ex.Message}");
                result.IsSuccess = false;
                result.ErrorMessage = $"Failed to export PDF: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Opens the exported PDF file with the default PDF viewer.
        /// </summary>
        public async Task<bool> OpenPdfAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[PdfExportService] PDF file not found: {filePath}");
                    return false;
                }

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportService] Error opening PDF: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets list of previously exported PDF files.
        /// </summary>
        public List<FileInfo> GetExportedFiles()
        {
            try
            {
                string exportsPath = Path.Combine(FileSystem.AppDataDirectory, EXPORTS_FOLDER);
                
                if (!Directory.Exists(exportsPath))
                {
                    return new List<FileInfo>();
                }

                var files = new DirectoryInfo(exportsPath)
                    .GetFiles("*.pdf")
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                return files;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportService] Error getting exported files: {ex.Message}");
                return new List<FileInfo>();
            }
        }

        /// <summary>
        /// Deletes an exported PDF file.
        /// </summary>
        public bool DeleteExportedFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"[PdfExportService] Deleted file: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfExportService] Error deleting file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates the PDF document from journal entries.
        /// </summary>
        private void GeneratePdf(string filePath, List<JournalEntry> entries, DateTime startDate, DateTime endDate)
        {
            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                // Set document margins
                document.SetMargins(36, 36, 36, 36);

                // Title
                var title = new Paragraph("Journal Export")
                    .SetFontSize(24)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(title);

                // Date range info
                var dateRangeInfo = new Paragraph($"{startDate:MMMM d, yyyy} - {endDate:MMMM d, yyyy}")
                    .SetFontSize(12)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(dateRangeInfo);

                // Entry count info
                var countInfo = new Paragraph($"{entries.Count} {(entries.Count == 1 ? "entry" : "entries")}")
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(20);
                document.Add(countInfo);

                // Export date
                var exportDate = new Paragraph($"Exported on {DateTime.Now:MMMM d, yyyy 'at' h:mm tt}")
                    .SetFontSize(9)
                    .SetItalic()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(20);
                document.Add(exportDate);

                // Separator line
                var separator = new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine())
                    .SetMarginBottom(20);
                document.Add(separator);

                // Add entries
                foreach (var entry in entries)
                {
                    AddEntryToPdf(document, entry);
                }

                document.Close();
            }
        }

        /// <summary>
        /// Adds a single journal entry to the PDF document.
        /// </summary>
        private void AddEntryToPdf(Document document, JournalEntry entry)
        {
            // Entry date
            var entryDate = new Paragraph(entry.EntryDate.ToString("dddd, MMMM d, yyyy"))
                .SetFontSize(14)
                .SetBold()
                .SetMarginTop(15)
                .SetMarginBottom(5);
            document.Add(entryDate);

            // Mood badge
            if (entry.EntryMoods != null && entry.EntryMoods.Any())
            {
                var moodNames = string.Join(", ", entry.EntryMoods.Select(m => m.Mood?.Name ?? "Unknown"));
                var moodPara = new Paragraph($"Mood: {moodNames}")
                    .SetFontSize(10)
                    .SetItalic()
                    .SetMarginBottom(8);
                document.Add(moodPara);
            }

            // Title
            if (!string.IsNullOrEmpty(entry.Title))
            {
                var title = new Paragraph(entry.Title)
                    .SetFontSize(12)
                    .SetBold()
                    .SetMarginBottom(8);
                document.Add(title);
            }

            // Content
            if (!string.IsNullOrEmpty(entry.Content))
            {
                var content = new Paragraph(entry.Content)
                    .SetFontSize(11)
                    .SetMarginBottom(10);
                document.Add(content);
            }

            // Tags
            if (entry.EntryTags != null && entry.EntryTags.Any())
            {
                var tagText = string.Join(", ", entry.EntryTags.Select(t => t.Tag?.Name ?? "Unknown"));
                var tags = new Paragraph($"Tags: {tagText}")
                    .SetFontSize(9)
                    .SetItalic()
                    .SetMarginBottom(15);
                document.Add(tags);
            }

            // Separator
            var separator = new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine(0.5f))
                .SetMarginBottom(10);
            document.Add(separator);
        }

        /// <summary>
        /// Generates a descriptive file name for the PDF export.
        /// </summary>
        private string GenerateFileName(DateTime startDate, DateTime endDate)
        {
            string startStr = startDate.ToString("MMM dd");
            string endStr = endDate.ToString("MMM dd");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");

            return $"JournalEntries_{startStr}-{endStr}_{timestamp}.pdf";
        }
    }
}
