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
        private string _searchQuery = string.Empty;
        private DateTime? _filterDateFrom;
        private DateTime? _filterDateTo;
        private List<string> _selectedMoods = new();
        private List<string> _selectedTags = new();
        private List<string> _availableMoods = new();
        private List<string> _availableTags = new();

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

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set
            {
                if (SetProperty(ref _filterDateFrom, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set
            {
                if (SetProperty(ref _filterDateTo, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public List<string> SelectedMoods
        {
            get => _selectedMoods;
            set
            {
                if (SetProperty(ref _selectedMoods, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public List<string> SelectedTags
        {
            get => _selectedTags;
            set
            {
                if (SetProperty(ref _selectedTags, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public List<string> AvailableMoods
        {
            get => _availableMoods;
            set => SetProperty(ref _availableMoods, value);
        }

        public List<string> AvailableTags
        {
            get => _availableTags;
            set => SetProperty(ref _availableTags, value);
        }

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        private List<JournalEntry> _allEntries = new();

        public JournalListViewModel(JournalService journalService)
        {
            _journalService = journalService;
            _currentPageIndex = 0;
            _totalPages = 0;

            NextPageCommand = new Command(async () => await GoToNextPageAsync(), () => HasNextPage);
            PreviousPageCommand = new Command(async () => await GoToPreviousPageAsync(), () => HasPreviousPage);
            RefreshCommand = new Command(async () => await LoadEntriesAsync());
            ClearFiltersCommand = new Command(async () => await ClearFiltersAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadMoodsAndTagsAsync();
            await LoadEntriesAsync();
        }

        private async Task LoadMoodsAndTagsAsync()
        {
            try
            {
                AvailableMoods = await _journalService.GetAllMoodsAsync();
                AvailableTags = await _journalService.GetAllTagsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading moods/tags: {ex}");
            }
        }

        public async Task ApplyFiltersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                CurrentPageIndex = 0;

                // Search with filters
                _allEntries = await _journalService.SearchEntriesAsync(
                    _searchQuery,
                    _filterDateFrom,
                    _filterDateTo,
                    _selectedMoods.Any() ? _selectedMoods : null,
                    _selectedTags.Any() ? _selectedTags : null
                );

                // Calculate total pages
                TotalPages = (int)Math.Ceiling((double)_allEntries.Count / PageSize);
                if (TotalPages == 0) TotalPages = 1;

                LoadCurrentPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error applying filters: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Filter Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ClearFiltersAsync()
        {
            SearchQuery = string.Empty;
            FilterDateFrom = null;
            FilterDateTo = null;
            SelectedMoods = new();
            SelectedTags = new();
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
