using ClipSyncWindows.Helpers;
using ClipSyncWindows.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace ClipSyncWindows.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ThemeManager ThemeManager { get; } = ThemeManager.Instance;

        public ICommand CloseSettingsCommand { get; }

        public event EventHandler? CloseSettingsRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            CloseSettingsCommand = new RelayCommand(_ => CloseSettings());
        }

        public void ToggleTheme()
        {
            ThemeManager.ToggleTheme();
        }

        private void CloseSettings()
        {
            CloseSettingsRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
