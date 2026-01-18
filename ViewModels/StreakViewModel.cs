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

        public ICommand RefreshCommand { get; }
        public ICommand ReloadCommand { get; }

        public StreakViewModel(DailyStreakService streakService)
        {
            _streakService = streakService;
            RefreshCommand = new Command(async () => await LoadStreakDataAsync());
            ReloadCommand = new Command(async () => await LoadStreakDataAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadStreakDataAsync();
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
    }
}
