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

        private ThemeManager()
        {
            var settings = SettingsService.LoadSettings();
            _currentTheme = settings.Theme;
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

                    // Save the new theme setting
                    var settings = SettingsService.LoadSettings();
                    settings.Theme = _currentTheme;
                    SettingsService.SaveSettings(settings);
                }
            }
        }

        public bool IsDarkTheme => CurrentTheme == AppTheme.Dark;

        // Light Theme Colors
        public SolidColorBrush WindowBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(18, 18, 18))
            : new SolidColorBrush(Color.FromRgb(250, 250, 250));

        public SolidColorBrush CardBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(30, 30, 30))
            : new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public SolidColorBrush ListBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(30, 30, 30))
            : new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public SolidColorBrush ListItemBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(30, 30, 30))
            : new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public SolidColorBrush ListItemSelectedBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 77, 64))
            : new SolidColorBrush(Color.FromRgb(178, 223, 219));

        public SolidColorBrush ListItemHoverBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(230, 81, 0))
            : new SolidColorBrush(Color.FromRgb(255, 224, 178));

        public SolidColorBrush BorderBrush => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 77, 64))
            : new SolidColorBrush(Color.FromRgb(178, 223, 219));

        public SolidColorBrush ListItemSelectedBorder => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(72, 169, 153))
            : new SolidColorBrush(Color.FromRgb(0, 121, 107));

        public SolidColorBrush ListItemHoverBorder => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(255, 173, 66))
            : new SolidColorBrush(Color.FromRgb(245, 124, 0));

        public SolidColorBrush PrimaryText => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(230, 225, 229))
            : new SolidColorBrush(Color.FromRgb(28, 27, 31));

        public SolidColorBrush SecondaryText => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(230, 225, 229))
            : new SolidColorBrush(Color.FromRgb(28, 27, 31));

        public SolidColorBrush StatusBarBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(72, 169, 153))
            : new SolidColorBrush(Color.FromRgb(0, 121, 107));

        public SolidColorBrush IconBackground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(72, 169, 153))
            : new SolidColorBrush(Color.FromRgb(0, 121, 107));

        public SolidColorBrush IconForeground => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 56, 47))
            : new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public SolidColorBrush StatusBarEllipseColor => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(230, 81, 0))
            : new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public SolidColorBrush ShareButtonColor => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 77, 64))
            : new SolidColorBrush(Color.FromRgb(229, 247, 245));

        public SolidColorBrush ShareButtonHoverColor => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(0, 51, 44))
            : new SolidColorBrush(Color.FromRgb(210, 239, 236));

        public SolidColorBrush ShareButtonForegroundColor => IsDarkTheme
            ? new SolidColorBrush(Color.FromRgb(229, 247, 245))
            : new SolidColorBrush(Color.FromRgb(0, 77, 64));

        public LinearGradientBrush TitleGradient => IsDarkTheme
            ? new LinearGradientBrush(
                Color.FromRgb(72, 169, 153),
                Color.FromRgb(33, 150, 243),
               new Point(0, 0), new Point(1, 0))
            : new LinearGradientBrush(
                Color.FromRgb(0, 121, 107),
                Color.FromRgb(3, 169, 244),
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
            OnPropertyChanged(nameof(StatusBarEllipseColor));
            OnPropertyChanged(nameof(ShareButtonColor));
            OnPropertyChanged(nameof(ShareButtonHoverColor));
            OnPropertyChanged(nameof(ShareButtonForegroundColor));
            OnPropertyChanged(nameof(TitleGradient));
            OnPropertyChanged(nameof(IsDarkTheme));
        }

        public void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
