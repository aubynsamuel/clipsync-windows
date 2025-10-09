
namespace ClipSyncWindows.Models
{
    public class ThemeSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;
    }

    public enum AppTheme
    {
        Light,
        Dark
    }
}
