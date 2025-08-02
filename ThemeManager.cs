using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace ClipSyncWindows
{
    public enum AppTheme
    {
        Light,
        Dark
    }

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
            ? new SolidColorBrush(Color.FromRgb(17, 24, 39))  // #111827
            : new SolidColorBrush(Color.FromRgb(245, 247, 250)); // #F5F7FA

        public SolidColorBrush CardBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(31, 41, 55))  // #1F2937
            : Brushes.White;

        public SolidColorBrush ListBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(55, 65, 81))  // #374151
            : new SolidColorBrush(Color.FromRgb(249, 250, 251)); // #F9FAFB

        public SolidColorBrush ListItemBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(75, 85, 99))  // #4B5563
            : Brushes.White;

        public SolidColorBrush ListItemSelectedBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(59, 130, 246))  // #3B82F6
            : new SolidColorBrush(Color.FromRgb(238, 242, 255)); // #EEF2FF

        public SolidColorBrush ListItemHoverBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(107, 114, 128))  // #6B7280
            : new SolidColorBrush(Color.FromRgb(249, 250, 251)); // #F9FAFB

        public SolidColorBrush BorderBrush => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(75, 85, 99))  // #4B5563
            : new SolidColorBrush(Color.FromRgb(229, 231, 235)); // #E5E7EB

        public SolidColorBrush ListItemSelectedBorder => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(59, 130, 246))  // #3B82F6
            : new SolidColorBrush(Color.FromRgb(99, 102, 241)); // #6366F1

        public SolidColorBrush ListItemHoverBorder => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(107, 114, 128))  // #6B7280
            : new SolidColorBrush(Color.FromRgb(209, 213, 219)); // #D1D5DB

        public SolidColorBrush PrimaryText => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(243, 244, 246))  // #F3F4F6
            : new SolidColorBrush(Color.FromRgb(17, 24, 39)); // #111827

        public SolidColorBrush SecondaryText => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(156, 163, 175))  // #9CA3AF
            : new SolidColorBrush(Color.FromRgb(107, 114, 128)); // #6B7280

        public SolidColorBrush StatusBarBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(55, 65, 81))  // #374151
            : new SolidColorBrush(Color.FromRgb(243, 244, 246)); // #F3F4F6

        public SolidColorBrush IconBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(59, 130, 246))  // #3B82F6
            : new SolidColorBrush(Color.FromRgb(238, 242, 255)); // #EEF2FF

        public SolidColorBrush IconForeground => IsDarkTheme
            ? Brushes.White
            : new SolidColorBrush(Color.FromRgb(99, 102, 241)); // #6366F1

        public LinearGradientBrush TitleGradient => IsDarkTheme
            ? new LinearGradientBrush(
                Color.FromRgb(59, 130, 246),  // #3B82F6
                Color.FromRgb(147, 51, 234),  // #9333EA
                new Point(0, 0), new Point(1, 0))
            : new LinearGradientBrush(
                Color.FromRgb(99, 102, 241),  // #6366F1
                Color.FromRgb(139, 92, 246),  // #8B5CF6
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

    public class ThemeSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;
    }
}