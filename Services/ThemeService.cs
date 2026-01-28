using System;
using System.Collections.Generic;
using JournalDaily.Models;
using Microsoft.Maui.Storage;

namespace JournalDaily.Services
{
    public class ThemeService
    {
        private Theme _currentTheme;
        private const string ThemePreferenceKey = "app_theme_preference";
        private const string CustomThemeKey = "app_custom_theme_colors";

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    SaveThemePreference();
                    OnThemeChanged(new ThemeChangedEventArgs(_currentTheme));
                }
            }
        }

        public List<string> AvailableThemes => new()
        {
            "Light",
            "Dark",
            "Sunset",
            "Ocean",
            "Forest",
            "Purple"
        };

        public ThemeService()
        {
            _currentTheme = LoadThemePreference();
        }

        public void SetTheme(string themeName)
        {
            CurrentTheme = themeName switch
            {
                "Light" => Theme.GetLightTheme(),
                "Dark" => Theme.GetDarkTheme(),
                _ => Theme.GetCustomTheme(themeName)
            };
        }

        public void SetCustomThemeColors(ThemeColors colors)
        {
            _currentTheme.Colors = colors;
            _currentTheme.Mode = ThemeMode.Custom;
            SaveThemePreference();
            OnThemeChanged(new ThemeChangedEventArgs(_currentTheme));
        }

        public ThemeColors GetCustomThemeColors()
        {
            return _currentTheme.Colors;
        }

        private Theme LoadThemePreference()
        {
            try
            {
                var savedTheme = SecureStorage.GetAsync(ThemePreferenceKey).Result;
                
                if (!string.IsNullOrEmpty(savedTheme))
                {
                    return savedTheme switch
                    {
                        "Light" => Theme.GetLightTheme(),
                        "Dark" => Theme.GetDarkTheme(),
                        _ => Theme.GetCustomTheme(savedTheme)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme preference: {ex}");
            }

            // Default to Light theme
            return Theme.GetLightTheme();
        }

        private void SaveThemePreference()
        {
            try
            {
                SecureStorage.SetAsync(ThemePreferenceKey, _currentTheme.Name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme preference: {ex}");
            }
        }

        protected virtual void OnThemeChanged(ThemeChangedEventArgs e)
        {
            ThemeChanged?.Invoke(this, e);
        }

        public string GetCssVariable(string colorName)
        {
            var colors = _currentTheme.Colors;
            return colorName switch
            {
                "primary" => colors.PrimaryColor,
                "primary-dark" => colors.PrimaryDark,
                "primary-light" => colors.PrimaryLight,
                "secondary" => colors.SecondaryColor,
                "secondary-dark" => colors.SecondaryDark,
                "secondary-light" => colors.SecondaryLight,
                "background" => colors.BackgroundColor,
                "surface" => colors.SurfaceColor,
                "border" => colors.BorderColor,
                "text" => colors.TextColor,
                "text-secondary" => colors.TextSecondaryColor,
                "text-tertiary" => colors.TextTertiaryColor,
                "success" => colors.SuccessColor,
                "warning" => colors.WarningColor,
                "error" => colors.ErrorColor,
                "info" => colors.InfoColor,
                "gradient-start" => colors.GradientStart,
                "gradient-end" => colors.GradientEnd,
                _ => "#000000"
            };
        }
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public Theme Theme { get; }

        public ThemeChangedEventArgs(Theme theme)
        {
            Theme = theme;
        }
    }
}
