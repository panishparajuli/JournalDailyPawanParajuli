using JournalDaily.Services;

namespace JournalDaily
{
    public partial class App : Application
    {
        private PinAuthService _pinAuthService;
        private bool _pinAuthenticated = false;

        public App()
        {
            InitializeComponent();

            // Get PIN service from DI container
            _pinAuthService = MauiProgram.Current.Services.GetService<PinAuthService>();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            base.OnStart();
            System.Diagnostics.Debug.WriteLine("[App] OnStart - Checking PIN authentication");

            // Check if PIN is required
            if (_pinAuthService.IsPinSetupComplete() && !_pinAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[App] PIN setup exists, navigating to PIN auth");
                Shell.Current?.GoToAsync("pin-auth", true);
            }
        }

        /// <summary>
        /// Called by PIN authentication pages to mark user as authenticated.
        /// </summary>
        public void SetPinAuthenticated(bool authenticated)
        {
            _pinAuthenticated = authenticated;
            if (authenticated)
            {
                System.Diagnostics.Debug.WriteLine("[App] PIN authentication successful");
            }
        }

        public bool IsPinAuthenticated => _pinAuthenticated;
    }
}
