using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JournalDaily.Data;
using JournalDaily.Services;
using JournalDaily.ViewModels;
using System.IO;
using Microsoft.Maui.Storage;

namespace JournalDaily
{
    public static class MauiProgram
    {
        public static MauiApp Current { get; private set; }

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Configure SQLite DbContext
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journaldaily.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<JournalService>();
            builder.Services.AddScoped<DailyStreakService>();
            builder.Services.AddScoped<PinAuthService>();
            builder.Services.AddScoped<PdfExportService>();
            builder.Services.AddScoped<CalendarViewModel>();
            builder.Services.AddScoped<JournalListViewModel>();
            builder.Services.AddScoped<StreakViewModel>();
            builder.Services.AddScoped<PinViewModel>();
            builder.Services.AddScoped<PdfExportViewModel>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Ensure database file and schema exist on first run
            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }
            catch
            {
                // swallow startup DB exceptions to avoid crashing the UI; errors will surface in logs
            }

            Current = app;
            return app;
        }
    }
}
