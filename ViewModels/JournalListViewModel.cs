using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using JournalDaily.Models;
using JournalDaily.Services;

namespace JournalDaily.ViewModels
{
    public class JournalListViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;
        private int _currentPageIndex;
        private int _totalPages;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _pageInfo = string.Empty;

        private const int PageSize = 5;

        public ObservableCollection<JournalEntry> CurrentPageEntries { get; } = new();

        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                if (SetProperty(ref _currentPageIndex, value))
                {
                    UpdatePageInfo();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string PageInfo
        {
            get => _pageInfo;
            set => SetProperty(ref _pageInfo, value);
        }

        public bool HasPreviousPage => CurrentPageIndex > 0;
        public bool HasNextPage => CurrentPageIndex < TotalPages - 1;

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }

        private List<JournalEntry> _allEntries = new();

        public JournalListViewModel(JournalService journalService)
        {
            _journalService = journalService;
            _currentPageIndex = 0;
            _totalPages = 0;

            NextPageCommand = new Command(async () => await GoToNextPageAsync(), () => HasNextPage);
            PreviousPageCommand = new Command(async () => await GoToPreviousPageAsync(), () => HasPreviousPage);
            RefreshCommand = new Command(async () => await LoadEntriesAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadEntriesAsync();
        }

        public async Task LoadEntriesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load all entries (ordered by date descending)
                _allEntries = await _journalService.GetAllEntriesAsync();

                // Calculate total pages
                TotalPages = (int)Math.Ceiling((double)_allEntries.Count / PageSize);
                if (TotalPages == 0) TotalPages = 1;

                // Reset to first page and load entries
                CurrentPageIndex = 0;
                LoadCurrentPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading entries: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"JournalList Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GoToNextPageAsync()
        {
            if (HasNextPage)
            {
                CurrentPageIndex++;
            }
        }

        private async Task GoToPreviousPageAsync()
        {
            if (HasPreviousPage)
            {
                CurrentPageIndex--;
            }
        }

        private void LoadCurrentPage()
        {
            CurrentPageEntries.Clear();

            var skip = CurrentPageIndex * PageSize;
            var pageEntries = _allEntries.Skip(skip).Take(PageSize).ToList();

            foreach (var entry in pageEntries)
            {
                CurrentPageEntries.Add(entry);
            }

            UpdatePageInfo();

            // Raise CanExecute changed for commands
            ((Command)NextPageCommand).ChangeCanExecute();
            ((Command)PreviousPageCommand).ChangeCanExecute();
        }

        private void UpdatePageInfo()
        {
            if (TotalPages == 0)
            {
                PageInfo = "No entries";
            }
            else
            {
                var startNumber = CurrentPageIndex * PageSize + 1;
                var endNumber = Math.Min((CurrentPageIndex + 1) * PageSize, _allEntries.Count);
                PageInfo = $"Showing {startNumber}-{endNumber} of {_allEntries.Count} entries (Page {CurrentPageIndex + 1} of {TotalPages})";
            }
        }
    }
}
