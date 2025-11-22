using ClipSyncWindows.Models;
using System.IO;

namespace ClipSyncWindows.Services
{
    public static class SettingsService
    {
        private static readonly string _settingsFilePath;

        static SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "ClipSync");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _settingsFilePath = Path.Combine(appFolder, "settings.json");
        }

        public static AppSettings LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new AppSettings(); // Return default settings if file doesn't exist
            }

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings(); // Return default settings on error
            }
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // Ignore save failures
            }
        }
    }
}
