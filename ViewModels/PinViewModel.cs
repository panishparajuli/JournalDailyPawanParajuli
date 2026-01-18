using System.Collections.ObjectModel;
using System.Windows.Input;
using JournalDaily.Services;

namespace JournalDaily.ViewModels
{
    /// <summary>
    /// MVVM ViewModel for PIN authentication and setup flows.
    /// Manages PIN entry state, validation, and interaction with PinAuthService.
    /// </summary>
    public class PinViewModel : BaseViewModel
    {
        private readonly PinAuthService _pinAuthService;
        private string _pinEntry = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private bool _showPinDots = true;
        private int _remainingAttempts = 5;
        private bool _isLockedOut = false;

        public PinViewModel(PinAuthService pinAuthService)
        {
            _pinAuthService = pinAuthService;
            _remainingAttempts = _pinAuthService.GetRemainingAttempts();
            _isLockedOut = _pinAuthService.IsLockedOut();

            // Initialize commands
            SubmitPinCommand = new Command(OnSubmitPin, CanSubmitPin);
            ClearPinCommand = new Command(OnClearPin);
            TogglePinVisibilityCommand = new Command(OnTogglePinVisibility);
            DeletePinDigitCommand = new Command(OnDeletePinDigit);
            ResetCommand = new Command(OnReset);
        }

        public string PinEntry
        {
            get => _pinEntry;
            set
            {
                if (SetProperty(ref _pinEntry, value))
                {
                    ((Command)SubmitPinCommand).ChangeCanExecute();
                    OnPropertyChanged(nameof(PinMaskDisplay));
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ShowPinDots
        {
            get => _showPinDots;
            set => SetProperty(ref _showPinDots, value);
        }

        public int RemainingAttempts
        {
            get => _remainingAttempts;
            set => SetProperty(ref _remainingAttempts, value);
        }

        public bool IsLockedOut
        {
            get => _isLockedOut;
            set => SetProperty(ref _isLockedOut, value);
        }

        /// <summary>
        /// Displays PIN as dots (●) or hidden when ShowPinDots is true.
        /// </summary>
        public string PinMaskDisplay => ShowPinDots ? new string('●', PinEntry.Length) : PinEntry;

        public ICommand SubmitPinCommand { get; }
        public ICommand ClearPinCommand { get; }
        public ICommand TogglePinVisibilityCommand { get; }
        public ICommand DeletePinDigitCommand { get; }
        public ICommand ResetCommand { get; }

        private bool CanSubmitPin()
        {
            return PinEntry.Length == 4 && !IsLoading && !IsLockedOut;
        }

        private async void OnSubmitPin()
        {
            if (!CanSubmitPin())
            {
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                bool isSetupMode = !_pinAuthService.IsPinSetupComplete();

                if (isSetupMode)
                {
                    // Setup mode - create new PIN
                    bool setupSuccess = await _pinAuthService.SetupPinAsync(PinEntry);

                    if (setupSuccess)
                    {
                        ErrorMessage = string.Empty;
                        System.Diagnostics.Debug.WriteLine("[PinViewModel] PIN setup successful.");
                        // Signal success - caller should navigate away
                        await Shell.Current.GoToAsync("///", true);
                    }
                    else
                    {
                        ErrorMessage = "Invalid PIN. Please use a 4-digit number.";
                    }
                }
                else
                {
                    // Verification mode - verify PIN
                    bool isValid = await _pinAuthService.VerifyPinAsync(PinEntry);

                    if (isValid)
                    {
                        ErrorMessage = string.Empty;
                        System.Diagnostics.Debug.WriteLine("[PinViewModel] PIN verification successful.");
                        // Signal success - caller should navigate away
                        await Shell.Current.GoToAsync("///", true);
                    }
                    else
                    {
                        RemainingAttempts = _pinAuthService.GetRemainingAttempts();
                        IsLockedOut = _pinAuthService.IsLockedOut();

                        if (IsLockedOut)
                        {
                            ErrorMessage = $"Too many attempts. Locked out for 15 minutes.";
                        }
                        else
                        {
                            ErrorMessage = $"Incorrect PIN. {RemainingAttempts} attempts remaining.";
                        }

                        PinEntry = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PinViewModel] Error submitting PIN: {ex.Message}");
                ErrorMessage = "An error occurred. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnClearPin()
        {
            PinEntry = string.Empty;
            ErrorMessage = string.Empty;
        }

        private void OnTogglePinVisibility()
        {
            ShowPinDots = !ShowPinDots;
        }

        private void OnDeletePinDigit()
        {
            if (PinEntry.Length > 0)
            {
                PinEntry = PinEntry.Substring(0, PinEntry.Length - 1);
            }
        }

        private async void OnReset()
        {
            if (await Shell.Current.DisplayAlert(
                "Reset PIN",
                "This will clear your PIN and require setup on next launch.",
                "Reset",
                "Cancel"))
            {
                await _pinAuthService.ResetPinAsync();
                PinEntry = string.Empty;
                ErrorMessage = string.Empty;
                IsLockedOut = false;
                RemainingAttempts = 5;
            }
        }

        /// <summary>
        /// Adds a digit to PIN entry (for numeric keypad).
        /// </summary>
        public void AddDigit(string digit)
        {
            if (PinEntry.Length < 4 && digit.All(char.IsDigit))
            {
                PinEntry += digit;
            }
        }

        /// <summary>
        /// Initialize view model for current mode (setup or verify).
        /// </summary>
        public void Initialize()
        {
            PinEntry = string.Empty;
            ErrorMessage = string.Empty;
            ShowPinDots = true;
            RemainingAttempts = _pinAuthService.GetRemainingAttempts();
            IsLockedOut = _pinAuthService.IsLockedOut();

            System.Diagnostics.Debug.WriteLine($"[PinViewModel] Initialized. Setup mode: {!_pinAuthService.IsPinSetupComplete()}, Locked out: {IsLockedOut}");
        }
    }
}
