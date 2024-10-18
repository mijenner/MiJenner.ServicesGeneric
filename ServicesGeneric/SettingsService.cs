using System.Diagnostics;
using System.Text.Json;

namespace MiJenner.ServicesGeneric
{
    /// <summary>
    /// SettingsService for user settings which may change frequently during runtime. 
    /// </summary>
    public class SettingsService : ISettingsService
    {
        // Bool to tell user if configuration file exists, if not use CreateSettingsFile. 
        private bool configExists = false;
        public bool ConfigExists { get { return configExists; } }

        // Configuration filename: 
        public string fileName = string.Empty;
        // Full folder path to but excluding "appsettings.json": 
        private string folderPath;
        public string FolderPath { get => folderPath; }
        // Full path to "appsettings.json" including filename: 
        private string filePath;
        public string FilePath { get => filePath; }

        // appname and companyname and folder policy 
        private string companyName = string.Empty;
        private string appName = string.Empty;
        private AppSettingsFolderPolicy appSettingsFolderPolicy;

        private Dictionary<string, object> settings;

        public SettingsService(AppSettingsFolderPolicy appSettingsFolderPolicy, string? companyName, string? appName, string? fileName)
        {
            this.fileName = (String.IsNullOrEmpty(fileName)) ? "settings.json" : fileName;
            this.appName = (String.IsNullOrEmpty(appName)) ? "appname" : appName;
            this.companyName = (String.IsNullOrEmpty(companyName)) ? "companyname" : companyName;
            this.appSettingsFolderPolicy = appSettingsFolderPolicy;

            settings = new Dictionary<string, object>();

            (folderPath, filePath) = HelpersGeneric.SetFileAndFolderPaths(appSettingsFolderPolicy, companyName, appName, fileName);
            configExists = HelpersGeneric.ConditionalCopyPackagedSettingsFile(appSettingsFolderPolicy, companyName, appName, fileName);

            LoadSettings();
        }

        public T Get<T>(string key, T defaultValue)
        {
            return settings.TryGetValue(key, out var value) ? (T)value : defaultValue;
        }

        public async Task Save<T>(string key, T value)
        {
            settings[key] = value;
            await SaveSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message); 
                    throw;
                }
            }
        }

        private async Task SaveSettings()
        {
            var json = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}
