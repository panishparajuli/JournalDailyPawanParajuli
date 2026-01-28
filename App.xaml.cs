using JournalDaily.Services;

namespace JournalDaily
{
    public partial class App : Application
    {
        private bool _pinAuthenticated = false;

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            base.OnStart();
            System.Diagnostics.Debug.WriteLine("[App] OnStart - Application started");
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
