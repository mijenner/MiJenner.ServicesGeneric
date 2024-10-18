using Microsoft.Extensions.Configuration;
using MiJenner.ServicesGeneric;
using System.Diagnostics;

namespace AppSettingsDemoConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Use IAppSettingsService implementation to ... ");
            Console.WriteLine("- see if appsettings.json already exists in target folder?");
            Console.WriteLine("- if positive use its settings");
            Console.WriteLine("- if negative:");
            Console.WriteLine("- see if one is available from source output folder");
            Console.WriteLine("- if positive copy it to target, and use it");
            Console.WriteLine("- if negative, signal that user should call CreateSettingsFile ... ");
            Console.WriteLine("- and then use that one");
            Console.WriteLine("");

            IAppSettingsService appSettingsService = new AppSettingsServiceConsole(AppSettingsFolderPolicy.PolicyAppDataLocal, "companyname", "appname", "appsettings.json"); 

            if (!appSettingsService.ConfigExists)
            {
                appSettingsService.CreateSettingsFile(new { ConfigString1 = "blam bam", ConfigString2 = "Hest" });
                Debug.WriteLine("*** Create json file from code"); 
            }
            IConfiguration configuration = appSettingsService.GetConfiguration();

            Console.WriteLine(configuration["CompanyName"] ?? "Company name not in config file");
            Console.WriteLine(configuration["configString"] ?? "configString not in config file");
            Console.WriteLine(configuration["fiskepind"] ?? "fiskepind not in config file");

            HelpersGeneric.WriteConfiguration(configuration); 
        }
    }
}
