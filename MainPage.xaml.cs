namespace JournalDaily
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            try
            {
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("[MainPage] Component initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPage] FATAL ERROR during initialization: {ex}");
                throw;
            }
        }
    }
}
