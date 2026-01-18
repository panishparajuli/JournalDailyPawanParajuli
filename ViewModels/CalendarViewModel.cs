using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using JournalDaily.Models;
using JournalDaily.Services;

namespace JournalDaily.ViewModels
{
    public class CalendarViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;
        private DateTime _currentMonth;
        private JournalEntry? _selectedEntry;
        private DateTime? _selectedDate;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public ObservableCollection<CalendarDay> CalendarDays { get; } = new();
        public ObservableCollection<JournalEntry> EntriesForMonth { get; } = new();

        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                if (SetProperty(ref _currentMonth, value))
                {
                    _ = LoadCalendarAsync();
                }
            }
        }

        public JournalEntry? SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty(ref _selectedEntry, value);
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
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

        public ICommand SelectDateCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand TodayCommand { get; }

        public CalendarViewModel(JournalService journalService)
        {
            _journalService = journalService;
            _currentMonth = DateTime.Today;

            SelectDateCommand = new Command<DateTime>(async (date) => await OnDateSelectedAsync(date));
            PreviousMonthCommand = new Command(() => CurrentMonth = CurrentMonth.AddMonths(-1));
            NextMonthCommand = new Command(() => CurrentMonth = CurrentMonth.AddMonths(1));
            TodayCommand = new Command(() => CurrentMonth = DateTime.Today);
        }

        public async Task InitializeAsync()
        {
            await LoadCalendarAsync();
        }

        private async Task LoadCalendarAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load all entries for the current month
                var entriesForMonth = await _journalService.GetEntriesByMonthAsync(CurrentMonth.Year, CurrentMonth.Month);

                // Create a set of dates with entries for quick lookup
                var datesWithEntries = entriesForMonth.Select(e => e.EntryDate.Date).ToHashSet();

                // Generate calendar days
                CalendarDays.Clear();
                var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

                for (var date = startDate; date <= lastDayOfMonth; date = date.AddDays(1))
                {
                    var isCurrentMonth = date.Month == CurrentMonth.Month;
                    var hasEntry = datesWithEntries.Contains(date.Date);
                    var journalEntry = entriesForMonth.FirstOrDefault(e => e.EntryDate.Date == date.Date);

                    CalendarDays.Add(new CalendarDay
                    {
                        Date = date,
                        IsCurrentMonth = isCurrentMonth,
                        HasEntry = hasEntry,
                        Entry = journalEntry
                    });
                }

                // Update the entries collection for other UI bindings if needed
                EntriesForMonth.Clear();
                foreach (var entry in entriesForMonth.OrderByDescending(e => e.EntryDate))
                {
                    EntriesForMonth.Add(entry);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading calendar: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Calendar Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnDateSelectedAsync(DateTime date)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SelectedDate = date;

                // Fetch the journal entry for the selected date
                var entry = await _journalService.GetEntryByDateAsync(date);
                SelectedEntry = entry;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading entry: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Date Selection Error: {ex}");
                SelectedEntry = null;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool HasEntry { get; set; }
        public JournalEntry? Entry { get; set; }
    }
}
