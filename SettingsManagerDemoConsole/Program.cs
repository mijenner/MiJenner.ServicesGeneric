using MiJenner.ServicesGeneric; 

namespace SettingsManagerDemoConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AppSettings appSettings = new AppSettings() { ConnString = "database.sqlite3", ApiString="anAPIstring", IsRunningOK = true };
            UserSettings userSettings = new UserSettings() { UserName="Bent", WantsNotifications=false };

            string SettingsFileName = "settings.json";
            string AppName = "My App Name";
            string CompanyName = "My Company Name";
            FolderPolicy folderPolicy = FolderPolicy.AppDataLocal;

            Console.WriteLine("*** Current settings, from code:");
            Console.WriteLine($"    - {nameof(userSettings.UserName)}: {userSettings.UserName}");
            Console.WriteLine($"    - {nameof(appSettings.ConnString)}: {appSettings.ConnString}");

            SettingsManager<AppSettings, UserSettings> settings = new SettingsManager<AppSettings, UserSettings>(SettingsFileName, AppName, CompanyName, folderPolicy, appSettings, userSettings);
            await settings.PrepareSettingsFileAsync(); 
            
            Console.WriteLine($"Target settings file is : {settings.ManagerSettings.FilePath}");
            Console.WriteLine("*** Now let us read from file:");
            Console.WriteLine($"    - {nameof(userSettings.UserName)}: {userSettings.UserName}");
            Console.WriteLine($"    - {nameof(appSettings.ConnString)}: {appSettings.ConnString}");

            Console.WriteLine("*** Now let us change and store:");
            userSettings.UserName = "John Doe";
            appSettings.ApiString = "Application Programming Interface"; 
            await settings.SaveSettingsToDiskAsync();
            Console.WriteLine("From local settings objects: ");
            Console.WriteLine($"    - {nameof(userSettings.UserName)}: {userSettings.UserName}");
            Console.WriteLine($"    - {nameof(appSettings.ConnString)}: {appSettings.ConnString}");
            Console.WriteLine("From central settings object:");
            Console.WriteLine($"    - {nameof(settings.UserSettings.UserName)}: {settings.UserSettings.UserName}");
            Console.WriteLine($"    - {nameof(settings.AppSettings.ConnString)}: {settings.AppSettings.ConnString}");

        }
    }

    public class AppSettings
    {
        public string ConnString { get; set; }
        public string ApiString { get; set; }
        public bool IsRunningOK { get; set; }
    }

    public class UserSettings
    {
        public string UserName { get; set; }
        public bool WantsNotifications { get; set; }    
    }
}
