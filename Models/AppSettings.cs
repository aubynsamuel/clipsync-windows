namespace ClipSyncWindows.Models
{
    public class AppSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;
        public bool AutoSyncEnabled { get; set; } = false;
    }

    public enum AppTheme
    {
        Light,
        Dark
    }
}
