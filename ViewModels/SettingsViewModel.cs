using ClipSyncWindows.Models;
using ClipSyncWindows.Services;
using System.ComponentModel;

namespace ClipSyncWindows.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppSettings _settings;

        public SettingsViewModel()
        {
            _settings = SettingsService.LoadSettings();
        }

        public static string AppVersion
        {
            get
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.0.0";
            }
        }

        public AppTheme Theme
        {
            get => _settings.Theme;
            set
            {
                if (_settings.Theme != value)
                {
                    _settings.Theme = value;
                    OnPropertyChanged(nameof(Theme));
                    ThemeManager.Instance.CurrentTheme = value;
                }
            }
        }

        public bool AutoSyncEnabled
        {
            get => _settings.AutoSyncEnabled;
            set
            {
                if (_settings.AutoSyncEnabled != value)
                {
                    _settings.AutoSyncEnabled = value;
                    OnPropertyChanged(nameof(AutoSyncEnabled));
                    Save();
                    AutoSyncChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<bool>? AutoSyncChanged;

        public void Save()
        {
            SettingsService.SaveSettings(_settings);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
