using System;

namespace JournalDaily.Models
{
    public enum ThemeMode
    {
        Light,
        Dark,
        Custom
    }

    public class ThemeColors
    {
        // Primary Colors
        public string PrimaryColor { get; set; } = "#007bff";
        public string PrimaryDark { get; set; } = "#0056b3";
        public string PrimaryLight { get; set; } = "#e7f1ff";

        // Secondary Colors
        public string SecondaryColor { get; set; } = "#6c757d";
        public string SecondaryDark { get; set; } = "#5a6268";
        public string SecondaryLight { get; set; } = "#f8f9fa";

        // Background Colors
        public string BackgroundColor { get; set; } = "#ffffff";
        public string SurfaceColor { get; set; } = "#f8f9fa";
        public string BorderColor { get; set; } = "#dee2e6";

        // Text Colors
        public string TextColor { get; set; } = "#333333";
        public string TextSecondaryColor { get; set; } = "#666666";
        public string TextTertiaryColor { get; set; } = "#999999";

        // Accent Colors
        public string SuccessColor { get; set; } = "#51cf66";
        public string WarningColor { get; set; } = "#ffd43b";
        public string ErrorColor { get; set; } = "#ff6b6b";
        public string InfoColor { get; set; } = "#3498db";

        // Gradient
        public string GradientStart { get; set; } = "#667eea";
        public string GradientEnd { get; set; } = "#764ba2";
    }

    public class Theme
    {
        public string Name { get; set; } = "Light";
        public ThemeMode Mode { get; set; } = ThemeMode.Light;
        public ThemeColors Colors { get; set; } = new();

        public static Theme GetLightTheme()
        {
            return new Theme
            {
                Name = "Light",
                Mode = ThemeMode.Light,
                Colors = new ThemeColors
                {
                    PrimaryColor = "#007bff",
                    PrimaryDark = "#0056b3",
                    PrimaryLight = "#e7f1ff",
                    SecondaryColor = "#6c757d",
                    SecondaryDark = "#5a6268",
                    SecondaryLight = "#f8f9fa",
                    BackgroundColor = "#ffffff",
                    SurfaceColor = "#f8f9fa",
                    BorderColor = "#dee2e6",
                    TextColor = "#333333",
                    TextSecondaryColor = "#666666",
                    TextTertiaryColor = "#999999",
                    SuccessColor = "#51cf66",
                    WarningColor = "#ffd43b",
                    ErrorColor = "#ff6b6b",
                    InfoColor = "#3498db",
                    GradientStart = "#667eea",
                    GradientEnd = "#764ba2"
                }
            };
        }

        public static Theme GetDarkTheme()
        {
            return new Theme
            {
                Name = "Dark",
                Mode = ThemeMode.Dark,
                Colors = new ThemeColors
                {
                    PrimaryColor = "#4a9eff",
                    PrimaryDark = "#2e7fd9",
                    PrimaryLight = "#1a3d5c",
                    SecondaryColor = "#b0b8c1",
                    SecondaryDark = "#8492a3",
                    SecondaryLight = "#2d3748",
                    BackgroundColor = "#1a1a1a",
                    SurfaceColor = "#2d2d2d",
                    BorderColor = "#444444",
                    TextColor = "#e8e8e8",
                    TextSecondaryColor = "#b8b8b8",
                    TextTertiaryColor = "#888888",
                    SuccessColor = "#48bb78",
                    WarningColor = "#ed8936",
                    ErrorColor = "#f56565",
                    InfoColor = "#4299e1",
                    GradientStart = "#667eea",
                    GradientEnd = "#764ba2"
                }
            };
        }

        public static Theme GetCustomTheme(string themeName)
        {
            // Return custom theme based on name
            return themeName switch
            {
                "Sunset" => GetSunsetTheme(),
                "Ocean" => GetOceanTheme(),
                "Forest" => GetForestTheme(),
                "Purple" => GetPurpleTheme(),
                _ => GetLightTheme()
            };
        }

        private static Theme GetSunsetTheme()
        {
            return new Theme
            {
                Name = "Sunset",
                Mode = ThemeMode.Custom,
                Colors = new ThemeColors
                {
                    PrimaryColor = "#ff6b35",
                    PrimaryDark = "#d94c1a",
                    PrimaryLight = "#ffe5d9",
                    SecondaryColor = "#f7931e",
                    SecondaryDark = "#c67317",
                    SecondaryLight = "#fff3e0",
                    BackgroundColor = "#fef8f3",
                    SurfaceColor = "#fff5eb",
                    BorderColor = "#ffe0cc",
                    TextColor = "#2c1810",
                    TextSecondaryColor = "#5c4033",
                    TextTertiaryColor = "#8d6e63",
                    SuccessColor = "#66bb6a",
                    WarningColor = "#ffa726",
                    ErrorColor = "#ef5350",
                    InfoColor = "#42a5f5",
                    GradientStart = "#ff6b35",
                    GradientEnd = "#f7931e"
                }
            };
        }

        private static Theme GetOceanTheme()
        {
            return new Theme
            {
                Name = "Ocean",
                Mode = ThemeMode.Custom,
                Colors = new ThemeColors
                {
                    PrimaryColor = "#0288d1",
                    PrimaryDark = "#01579b",
                    PrimaryLight = "#b3e5fc",
                    SecondaryColor = "#0097a7",
                    SecondaryDark = "#00695c",
                    SecondaryLight = "#b2dfdb",
                    BackgroundColor = "#e0f7fa",
                    SurfaceColor = "#f0f7fa",
                    BorderColor = "#80deea",
                    TextColor = "#00363a",
                    TextSecondaryColor = "#00695c",
                    TextTertiaryColor = "#4db6ac",
                    SuccessColor = "#26a69a",
                    WarningColor = "#ffa726",
                    ErrorColor = "#ef5350",
                    InfoColor = "#29b6f6",
                    GradientStart = "#0288d1",
                    GradientEnd = "#0097a7"
                }
            };
        }

        private static Theme GetForestTheme()
        {
            return new Theme
            {
                Name = "Forest",
                Mode = ThemeMode.Custom,
                Colors = new ThemeColors
                {
                    PrimaryColor = "#2e7d32",
                    PrimaryDark = "#1b5e20",
                    PrimaryLight = "#c8e6c9",
                    SecondaryColor = "#558b2f",
                    SecondaryDark = "#33691e",
                    SecondaryLight = "#dcedc8",
                    BackgroundColor = "#f1f8e9",
                    SurfaceColor = "#f5f5f0",
                    BorderColor = "#aed581",
                    TextColor = "#1b5e20",
                    TextSecondaryColor = "#33691e",
                    TextTertiaryColor = "#558b2f",
                    SuccessColor = "#43a047",
                    WarningColor = "#fbc02d",
                    ErrorColor = "#e53935",
                    InfoColor = "#1e88e5",
                    GradientStart = "#2e7d32",
                    GradientEnd = "#558b2f"
                }
            };
        }

        private static Theme GetPurpleTheme()
        {
            return new Theme
            {
                Name = "Purple",
                Mode = ThemeMode.Custom,
                Colors = new ThemeColors
                {
                    PrimaryColor = "#7c3aed",
                    PrimaryDark = "#5b21b6",
                    PrimaryLight = "#ede9fe",
                    SecondaryColor = "#a855f7",
                    SecondaryDark = "#7c3aed",
                    SecondaryLight = "#f3e8ff",
                    BackgroundColor = "#faf5ff",
                    SurfaceColor = "#faf5ff",
                    BorderColor = "#e9d5ff",
                    TextColor = "#4c1d95",
                    TextSecondaryColor = "#6b21a8",
                    TextTertiaryColor = "#9333ea",
                    SuccessColor = "#10b981",
                    WarningColor = "#f59e0b",
                    ErrorColor = "#ef4444",
                    InfoColor = "#3b82f6",
                    GradientStart = "#7c3aed",
                    GradientEnd = "#a855f7"
                }
            };
        }
    }
}
