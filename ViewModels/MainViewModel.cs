
using ClipSyncWindows.Services;
using System.ComponentModel;

namespace ClipSyncWindows.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ThemeManager ThemeManager { get; } = ThemeManager.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void ToggleTheme()
        {
            ThemeManager.ToggleTheme();
        }
    }
}
