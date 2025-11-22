namespace ClipSyncWindows.Models
{
    public class AppSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;
    }

    public enum AppTheme
    {
        Light,
        Dark
    }
}
