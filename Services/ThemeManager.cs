using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using ClipSyncWindows.Models;

namespace ClipSyncWindows.Services
{
    public class ThemeManager : INotifyPropertyChanged
    {
        private static ThemeManager? _instance;
        public static ThemeManager Instance => _instance ??= new ThemeManager();

        private AppTheme _currentTheme = AppTheme.Light;

        private static string ThemeSettingsFile => GetThemeSettingsPath();

        private static string GetThemeSettingsPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "ClipSync");

            // Create directory if it doesn't exist
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return Path.Combine(appFolder, "theme_settings.json");
        }

        private ThemeManager()
        {
            LoadThemeSettings();
        }

        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged(nameof(CurrentTheme));
                    ApplyTheme();
                    SaveThemeSettings();
                }
            }
        }

        public bool IsDarkTheme => CurrentTheme == AppTheme.Dark;

        // Light Theme Colors
        public SolidColorBrush WindowBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(18, 18, 18))    // md_dark_background
            : new SolidColorBrush(Color.FromRgb(250, 250, 250)); // md_light_background

        public SolidColorBrush CardBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(30, 30, 30))    // md_dark_surface
            : new SolidColorBrush(Color.FromRgb(255, 255, 255)); // md_light_surface

        public SolidColorBrush ListBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(30, 30, 30))    // md_dark_surface
            : new SolidColorBrush(Color.FromRgb(255, 255, 255)); // md_light_surface

        public SolidColorBrush ListItemBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(30, 30, 30))    // md_dark_surface
            : new SolidColorBrush(Color.FromRgb(255, 255, 255)); // md_light_surface

        public SolidColorBrush ListItemSelectedBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 77, 64))    // md_dark_primaryContainer
            : new SolidColorBrush(Color.FromRgb(178, 223, 219)); // md_light_primaryContainer

        public SolidColorBrush ListItemHoverBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(230, 81, 0))   // md_dark_secondaryContainer
            : new SolidColorBrush(Color.FromRgb(255, 224, 178)); // md_light_secondaryContainer

        public SolidColorBrush BorderBrush => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 77, 64))    // md_dark_primaryContainer
            : new SolidColorBrush(Color.FromRgb(178, 223, 219)); // md_light_primaryContainer

        public SolidColorBrush ListItemSelectedBorder => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(72, 169, 153)) // md_dark_primary
            : new SolidColorBrush(Color.FromRgb(0, 121, 107));  // md_light_primary

        public SolidColorBrush ListItemHoverBorder => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(255, 173, 66)) // md_dark_secondary
            : new SolidColorBrush(Color.FromRgb(245, 124, 0));  // md_light_secondary

        public SolidColorBrush PrimaryText => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(230, 225, 229)) // md_dark_onBackground
            : new SolidColorBrush(Color.FromRgb(28, 27, 31));   // md_light_onBackground

        public SolidColorBrush SecondaryText => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(230, 225, 229)) // md_dark_onSurface
            : new SolidColorBrush(Color.FromRgb(28, 27, 31));   // md_light_onSurface

        public SolidColorBrush StatusBarBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(72, 169, 153)) // md_dark_primary
            : new SolidColorBrush(Color.FromRgb(0, 121, 107));  // md_light_primary

        public SolidColorBrush IconBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(72, 169, 153)) // md_dark_primary
            : new SolidColorBrush(Color.FromRgb(0, 121, 107));  // md_light_primary

        public SolidColorBrush IconForeground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 56, 47))     // md_dark_onPrimary
            : new SolidColorBrush(Color.FromRgb(255, 255, 255)); // md_light_onPrimary

        public LinearGradientBrush TitleGradient => IsDarkTheme
            ? new LinearGradientBrush(
                Color.FromRgb(72, 169, 153),   // teal
                Color.FromRgb(33, 150, 243),   // material blue
               new Point(0, 0), new Point(1, 0))
            : new LinearGradientBrush(
                Color.FromRgb(0, 121, 107),    // teal
                Color.FromRgb(3, 169, 244),    // light blue
                new Point(0, 0), new Point(1, 0));


        private void ApplyTheme()
        {
            // Update all bound properties
            OnPropertyChanged(nameof(WindowBackground));
            OnPropertyChanged(nameof(CardBackground));
            OnPropertyChanged(nameof(ListBackground));
            OnPropertyChanged(nameof(ListItemBackground));
            OnPropertyChanged(nameof(ListItemSelectedBackground));
            OnPropertyChanged(nameof(ListItemHoverBackground));
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(ListItemSelectedBorder));
            OnPropertyChanged(nameof(ListItemHoverBorder));
            OnPropertyChanged(nameof(PrimaryText));
            OnPropertyChanged(nameof(SecondaryText));
            OnPropertyChanged(nameof(StatusBarBackground));
            OnPropertyChanged(nameof(IconBackground));
            OnPropertyChanged(nameof(IconForeground));
            OnPropertyChanged(nameof(TitleGradient));
            OnPropertyChanged(nameof(IsDarkTheme));
        }

        public void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        }

        private void LoadThemeSettings()
        {
            try
            {
                if (File.Exists(ThemeSettingsFile))
                {
                    var json = File.ReadAllText(ThemeSettingsFile);
                    var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ThemeSettings>(json);
                    if (settings != null)
                    {
                        CurrentTheme = settings.Theme;
                    }
                }
            }
            catch
            {
                // If loading fails, stick with default light theme
            }
        }

        private void SaveThemeSettings()
        {
            try
            {
                var settings = new ThemeSettings { Theme = CurrentTheme };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
                File.WriteAllText(ThemeSettingsFile, json);
            }
            catch
            {
                // Ignore save failures
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
