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
