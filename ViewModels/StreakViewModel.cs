using System;
using System.Windows.Input;
using JournalDaily.Models;
using JournalDaily.Services;

namespace JournalDaily.ViewModels
{
    public class StreakViewModel : BaseViewModel
    {
        private readonly DailyStreakService _streakService;
        private StreakResult? _streakData;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _selectedView = "overview"; // overview or calendar
        private List<DateTime> _currentMonthDates = new();
        private DateTime _currentViewMonth;

        public StreakResult? StreakData
        {
            get => _streakData;
            set => SetProperty(ref _streakData, value);
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

        public string SelectedView
        {
            get => _selectedView;
            set => SetProperty(ref _selectedView, value);
        }

        public List<DateTime> CurrentMonthDates
        {
            get => _currentMonthDates;
            set => SetProperty(ref _currentMonthDates, value);
        }

        public DateTime CurrentViewMonth
        {
            get => _currentViewMonth;
            set => SetProperty(ref _currentViewMonth, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SwitchViewCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }

        public StreakViewModel(DailyStreakService streakService)
        {
            _streakService = streakService;
            _currentViewMonth = DateTime.Today;
            RefreshCommand = new Command(async () => await LoadStreakDataAsync());
            ReloadCommand = new Command(async () => await LoadStreakDataAsync());
            SwitchViewCommand = new Command<string?>(async (view) => await SwitchViewAsync(view));
            PreviousMonthCommand = new Command(async () => await PreviousMonthAsync());
            NextMonthCommand = new Command(async () => await NextMonthAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadStreakDataAsync();
            await LoadCurrentMonthDatesAsync();
        }

        private async Task LoadStreakDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                StreakData = await _streakService.CalculateStreakAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error calculating streak: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"StreakViewModel Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCurrentMonthDatesAsync()
        {
            try
            {
                var fromDate = new DateTime(CurrentViewMonth.Year, CurrentViewMonth.Month, 1);
                var toDate = fromDate.AddMonths(1).AddDays(-1);

                var monthDates = await _streakService.GetEntryDatesInRangeAsync(fromDate, toDate);
                CurrentMonthDates = monthDates;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading month dates: {ex}");
            }
        }

        private async Task SwitchViewAsync(string? view)
        {
            if (!string.IsNullOrEmpty(view))
            {
                SelectedView = view;
                if (view == "calendar")
                {
                    await LoadCurrentMonthDatesAsync();
                }
            }
        }

        private async Task PreviousMonthAsync()
        {
            CurrentViewMonth = CurrentViewMonth.AddMonths(-1);
            await LoadCurrentMonthDatesAsync();
        }

        private async Task NextMonthAsync()
        {
            // Don't go into the future beyond today's month
            if (CurrentViewMonth.AddMonths(1).Month <= DateTime.Today.Month || 
                CurrentViewMonth.AddMonths(1).Year < DateTime.Today.Year)
            {
                CurrentViewMonth = CurrentViewMonth.AddMonths(1);
                await LoadCurrentMonthDatesAsync();
            }
        }

        public bool HasEntryOnDate(DateTime date)
        {
            return CurrentMonthDates.Any(d => d.Date == date.Date);
        }

        public bool IsMissedDay(DateTime date)
        {
            return StreakData?.MissedDaysList.Any(d => d.Date == date.Date) ?? false;
        }

        public bool IsToday(DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        public bool IsFutureDate(DateTime date)
        {
            return date.Date > DateTime.Today;
        }
    }
}
