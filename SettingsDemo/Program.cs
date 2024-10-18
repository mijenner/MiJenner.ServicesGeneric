using MiJenner.ServicesGeneric;

namespace SettingsDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ISettingsService settingsService = new SettingsService(AppSettingsFolderPolicy.PolicyAppDataLocal, "companyname", "appname", "settings.json");

            settingsService.Save<string>("ConnectionString", "Path Source db3");
            settingsService.Save<bool>("WantsNews", true);

            var connString = settingsService.Get<string>("ConnectionString", "nope");
            var wantsNews = settingsService.Get<bool>("WantsNews", false);

            Console.WriteLine($"Connection string read was {connString}");
            Console.WriteLine($"Wants news read was {wantsNews}");
        }
    }
}
