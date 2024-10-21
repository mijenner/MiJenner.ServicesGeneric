using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace MiJenner.ServicesGeneric
{
    public class SettingsManager<TAppSettings, TUserSettings>
        where TAppSettings : class, new()
        where TUserSettings : class, new()
    {
        public SettingsMO ManagerSettings { get; private set; }
        public TAppSettings AppSettings { get => appSettings; private set => appSettings = value; }
        public TUserSettings UserSettings { get => userSettings; private set => userSettings = value; }

        private const string ErrorLogFilePath = "setmgm-errorlogs.txt";
        private TAppSettings appSettings;
        private TUserSettings userSettings;

        public SettingsManager(string? settingsFileName, string? appName, string? companyName, FolderPolicy? folderPolicy, TAppSettings appSettings, TUserSettings userSettings)
        {
            this.appSettings = appSettings;
            this.userSettings = userSettings;

            if (settingsFileName == ErrorLogFilePath)
            {
                var msg = $"settingsFileName not allowed to be {ErrorLogFilePath}, which is reserved";
                LogError(msg);
                throw new Exception(msg);
            }
            ManagerSettings = new SettingsMO
            {
                FileName = string.IsNullOrEmpty(settingsFileName) ? "appsettings.json" : settingsFileName,
                AppName = string.IsNullOrEmpty(appName) ? "appname" : appName,
                CompanyName = string.IsNullOrEmpty(companyName) ? "companyname" : companyName,
                FolderPolicy = folderPolicy ?? FolderPolicy.AppDataLocal
            };

            // Initialize ManagerSettings with paths, appname, etc.
            string folderPath, folderPathCache, filePath;
            (folderPath, folderPathCache, filePath) = DetermineFolderPath(ManagerSettings);
            ManagerSettings.FolderPath = folderPath;
            ManagerSettings.FolderPathCache = folderPathCache;
            ManagerSettings.FilePath = filePath;
        }

        private (string, string, string) DetermineFolderPath(SettingsMO settings)
        {
            string appName4Folder = HelpersGeneric.ChangeString2FolderName(settings.AppName);
            string companyName4Folder = HelpersGeneric.ChangeString2FolderName(settings.CompanyName);
            string folderPath, filePath, folderPathCache;

            switch (settings.FolderPolicy)
            {
                case FolderPolicy.Documents:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    break;
                case FolderPolicy.AppDataRoaming:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                case FolderPolicy.Desktop:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    break;
                case FolderPolicy.Unknown:
                case FolderPolicy.AppDataLocal:
                default:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    break;
            }
            folderPath = Path.Combine(folderPath, companyName4Folder, appName4Folder);
            filePath = Path.Combine(folderPath, settings.FileName);
            folderPathCache = folderPath;
            return (folderPath, folderPathCache, filePath);
        }

        private TSettings GetSettingsCopy<TSettings>(TSettings settings) where TSettings : class
        {
            if (typeof(ISettingsMOInitializable).IsAssignableFrom(settings.GetType()))
            {
                var initializableSettings = (ISettingsMOInitializable)settings;
                return (TSettings)initializableSettings.CopyForStorage();
            }
            return settings;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (typeof(ISettingsMOInitializable).IsAssignableFrom(AppSettings.GetType()))
                {
                    var initializableAppSettings = (ISettingsMOInitializable)AppSettings;
                    await initializableAppSettings.InitializeAsync();
                }

                if (typeof(ISettingsMOInitializable).IsAssignableFrom(UserSettings.GetType()))
                {
                    var initializableUserSettings = (ISettingsMOInitializable)UserSettings;
                    await initializableUserSettings.InitializeAsync();
                }
            }
            catch (Exception ex)
            {
                var msg = $"Error calling InitializeAsync() in a settings object: \n{ex.Message}";
                LogError(msg);
                throw new Exception(msg);
            }
        }

        public async Task SaveSettingsToDiskAsync()
        {
            TAppSettings appSettingsCopy = GetSettingsCopy<TAppSettings>(AppSettings);
            TUserSettings userSettingsCopy = GetSettingsCopy<TUserSettings>(UserSettings);

            // Serialize the non-sensitive copies
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            string json = JsonSerializer.Serialize(new
            {
                ManagerSettings = this.ManagerSettings,
                AppSettings = appSettingsCopy,
                UserSettings = userSettingsCopy
            }, options);

            // Write to file: 
            try
            {
                Directory.CreateDirectory(ManagerSettings.FolderPath);
            }
            catch (Exception ex)
            {
                var msg = $"Error trying to create folder {ManagerSettings.FolderPath}: \n{ex.Message}";
                LogError(msg);
                throw new Exception(msg);
            }
            try
            {
                await File.WriteAllTextAsync(ManagerSettings.FilePath, json);
            }
            catch (Exception ex)
            {
                var msg = $"Error trying to generate json file {ManagerSettings.FilePath}: \n{ex.Message}";
                LogError(msg);
                throw new Exception(msg);
            }
        }

        public async Task PrepareSettingsFileAsync()
        {
            string folderPath = ManagerSettings.FolderPath;
            string filePath = ManagerSettings.FilePath;
            string fileName = ManagerSettings.FileName;

            // Check if the file exists in the target location
            if (File.Exists(filePath))
            {
                Debug.WriteLine($"*** {filePath} exists and will be used");
                await LoadSettingsFromFileAsync();
                return;
            }

            // Check if packaged file exists, and use it
            string sourceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(sourceFilePath))
            {
                // Create the target directory if it doesn't exist
                Directory.CreateDirectory(folderPath);

                // Copy the packaged file to the target location
                File.Copy(sourceFilePath, filePath, true); // Overwrite if exists
                Debug.WriteLine($"*** {filePath} doesn't exist ... ");
                Debug.WriteLine($"*** copy {sourceFilePath} to {folderPath}");

                await LoadSettingsFromFileAsync();
                return;
            }

            // If no file exists, 
            Debug.WriteLine($"*** No {fileName} exists neither in {folderPath} nor in {sourceFilePath}");
            Debug.WriteLine("*** Writes a new settings file");
            await SaveSettingsToDiskAsync();
            await LoadSettingsFromFileAsync();

            return;
        }

        private void CopySettings<T>(T source, T target) where T : class
        {
            if (source == null || target == null)
            {
                throw new ArgumentNullException(nameof(source), "Source or target cannot be null.");
            }

            var type = typeof(T);
            foreach (var property in type.GetProperties())
            {
                // Copy only properties that can be read and written
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(source);
                    property.SetValue(target, value);
                }
            }
        }

        private async Task LoadSettingsFromFileAsync()
        {
            var filePath = ManagerSettings.FilePath;
            string json = string.Empty;
            TAppSettings appSettingsFromFile = new TAppSettings();
            TUserSettings userSettingsFromFile = new TUserSettings();

            // Read from file into json string: 
            try
            {
                json = await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                var msg = $"Cannot read file {filePath} when trying to load settings\n{ex}";
                LogError(msg);
                throw new Exception(msg);
            }

            var options = new JsonSerializerOptions
            {
                // PropertyNameCaseInsensitive = true // Optional: allows case-insensitive matching
            };

            try
            {
                // Assuming your JSON structure matches the properties of TAppSettings
                var jsonDocument = JsonDocument.Parse(json);
                var appSettingsJson = jsonDocument.RootElement.GetProperty("AppSettings").GetRawText();
                appSettingsFromFile = JsonSerializer.Deserialize<TAppSettings>(appSettingsJson, options);

                // Overwrite AppSettings if appSettingsFromFile aren't null
                if (appSettingsFromFile != null)
                {
                    CopySettings<TAppSettings>(appSettingsFromFile, appSettings); 
                }
                else
                {
                    var msg = $"Cannot read AppSettings from json-string {json}";
                    LogError(msg);
                    throw new Exception(msg);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Cannot read AppSettings from json-string {json}";
                LogError(msg);
                throw new Exception(msg);
            }

            try
            {
                // Assuming your JSON structure matches the properties of TUserSettings
                var jsonDocument = JsonDocument.Parse(json);
                var userSettingsJson = jsonDocument.RootElement.GetProperty("UserSettings").GetRawText();
                userSettingsFromFile = JsonSerializer.Deserialize<TUserSettings>(userSettingsJson, options);
                if (userSettingsFromFile != null)
                {
                    CopySettings<TUserSettings>(userSettingsFromFile, userSettings);
                }
                else
                {
                    var msg = $"Cannot read UserSettings from json-string {json}";
                    LogError(msg);
                    throw new Exception(msg);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Cannot read UserSettings from json-string {json}";
                LogError(msg);
                throw new Exception(msg);
            }

        }



        private void LogError(string message)
        {
            return;

            try
            {
                using (StreamWriter writer = new StreamWriter(ErrorLogFilePath, true))
                {
                    writer.WriteLine($"{DateTime.UtcNow}: {message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to log error: {ex.Message}");
            }
        }

    }
}
