using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MiJenner.ServicesGeneric;
using System.Diagnostics;

/// <summary>
/// AppSettingsService strives to make it easier to store application settings
/// across Console, Maui and Web-API projects. 
/// 
/// AppSettingsServiceConsole has the following policy: 
/// - Use JSON file as configuration-provider. 
/// - Receive users folder policy (Documents, AppDataLocal, AppData (roaming), or ... 
/// - Receive AppName and CompanyName. 
/// - Create configuration folder if it doesn't exists. 
/// - See if appsettings.json file is present already. If it is, use it. 
/// - If not present, attempt to copy from packaged appsettings.json 
/// - If not create one based on user supplied info. 
/// Finally: Offer IConfiguration object for handling configurations using JSON. 
/// 
/// Usage: 
/// AppSettingsService appSettingsService = AppSettingsService(AppSettingsFolderPolicy.AppDataLocal, "Company Name", "App Name", "appsettings.json")
/// 
/// Then 
/// 
/// IConfiguration config = appSettingsService.GetConfiguration(); 
/// 
/// </summary>
public class AppSettingsServiceConsole : IAppSettingsService
{
    // IConfiguration object: 
    private IConfiguration configuration;
    // Bool to tell user if configuration file exists, if not use CreateSettingsFile. 
    private bool configExists = false;
    public bool ConfigExists { get { return configExists; } }

    // Configuration filename: 
    public string fileName = "appsettings.json";
    // Full folder path to but excluding "appsettings.json": 
    private string folderPath;
    public string FolderPath { get => folderPath; }
    // Full path to "appsettings.json" including filename: 
    private string filePath;
    public string FilePath { get => filePath; }

    // appname and companyname and folder policy 
    private string companyName = "companyname";
    private string appName = "appname";
    private AppSettingsFolderPolicy appSettingsFolderPolicy;

    public AppSettingsServiceConsole(AppSettingsFolderPolicy appSettingsFolderPolicy, string? companyName, string? appName, string? fileName = "appsettings.json")
    {
        // Set filename and path 
        this.fileName = (String.IsNullOrEmpty(fileName)) ? "appsettings.json" : fileName;
        this.appName = (String.IsNullOrEmpty(appName)) ? "appname" : appName;
        this.companyName = (String.IsNullOrEmpty(companyName)) ? "companyname" : companyName;
        this.appSettingsFolderPolicy = appSettingsFolderPolicy;

        (folderPath, filePath) = HelpersGeneric.SetFileAndFolderPaths(appSettingsFolderPolicy, companyName, appName, fileName);
        configExists = HelpersGeneric.ConditionalCopyPackagedSettingsFile(appSettingsFolderPolicy, companyName, appName, fileName); 

        // Build a configuration object: 
        if (String.IsNullOrEmpty(FolderPath))
        {
            Debug.WriteLine("FolderPath not correctly set");
            throw new DirectoryNotFoundException($"Folder path {FolderPath} not valid"); 
        }

        RefreshConfiguration();
    }

    public void RefreshConfiguration()
    {
        if (!File.Exists(filePath))
        {
            configExists = false;
            return;
        } // todo 

        var builder = new ConfigurationBuilder()
            .SetBasePath(FolderPath)
            .AddJsonFile(this.fileName, optional: false, reloadOnChange: true);

        configuration = builder.Build();
    }

    public IConfiguration GetConfiguration()
    {
        return configuration;
    }

    public string GetFilePath()
    {
        return FilePath;
    }

    public string GetFolderPath()
    {
        return FolderPath;
    }

    public void CreateSettingsFile(object aSettingsObject)
    {
        if (aSettingsObject == null)
        {
            aSettingsObject = new { ASetting = "value" }; // creates anonymous object example setting. 
        }

        // Serialize the default settings to JSON
        string json = JsonSerializer.Serialize(aSettingsObject, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        RefreshConfiguration();
    }
}
