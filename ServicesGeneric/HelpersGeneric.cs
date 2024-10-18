using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace MiJenner.ServicesGeneric
{
    public static class HelpersGeneric
    {
        /// <summary>
        /// Sets folder and file paths according to folder policy, Documents, AppDataLocal or AppData (roaming). 
        /// </summary>
        /// <param name="appSettingsFolderPolicy"></param>
        /// <param name="companyName"></param>
        /// <param name="appName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static (string folderPath, string filePath) SetFileAndFolderPaths(AppSettingsFolderPolicy appSettingsFolderPolicy, string? companyName, string? appName, string? fileName)
        {
            string folderPath;
            string filePath;

            switch (appSettingsFolderPolicy)
            {
                case AppSettingsFolderPolicy.PolicyDocument:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    break;
                case AppSettingsFolderPolicy.PolicyAppDataRoaming:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                case AppSettingsFolderPolicy.PolicyDesktop:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    break;
                case AppSettingsFolderPolicy.Unknown:
                case AppSettingsFolderPolicy.PolicyAppDataLocal:
                default:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    break;
            }
            // if fileName, appName, companyName isn't given, set default value: 
            fileName = (String.IsNullOrEmpty(fileName)) ? "settings.json" : fileName;
            appName = (String.IsNullOrEmpty(appName)) ? "appname" : appName;
            companyName = (String.IsNullOrEmpty(companyName)) ? "companyname" : companyName;
            folderPath = Path.Combine(folderPath, companyName, appName);
            filePath = Path.Combine(folderPath, fileName);
            return (folderPath, filePath);
        }

        /// <summary>
        /// Tries to identify settingsfile. 
        /// 1) It searches in the "normal location", set by folder policy, company-name, 
        /// app-name and file-name. 
        /// 2) It then tries to copy it from AppDomain.CurrentDomain.BaseDirectory (copy to output) 
        /// 
        /// If found it returns true, else false. 
        /// </summary>
        /// <param name="appSettingsFolderPolicy"></param>
        /// <param name="companyName"></param>
        /// <param name="appName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ConditionalCopyPackagedSettingsFile(AppSettingsFolderPolicy appSettingsFolderPolicy, string? companyName, string? appName, string? fileName)
        {
            string? folderPath;
            string? filePath;
            bool configExists = false;

            // if fileName, appName, companyName isn't given, set default value: 
            fileName = (String.IsNullOrEmpty(fileName)) ? "settings.json" : fileName;
            appName = (String.IsNullOrEmpty(appName)) ? "appname" : appName;
            companyName = (String.IsNullOrEmpty(companyName)) ? "companyname" : companyName;

            (folderPath, filePath) = SetFileAndFolderPaths(appSettingsFolderPolicy, companyName, appName, fileName);

            // Check if the target directory exists, and create it if not
            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Problem creating directory {folderPath}"); 
                throw;
            }
            Debug.WriteLine($"*** Folder {folderPath} exists"); 

            // Check if the fileName exists in the target location
            if (File.Exists(filePath))
            {
                configExists = true;
                Debug.WriteLine($"*** {filePath} exists and will be used");
                return configExists;
            } 

            // Check if packaged fileName exist, and use it: 
            // Source (output) folder where the project-fileName will be after build
            string sourceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(sourceFilePath))
            {
                File.Copy(sourceFilePath, filePath);
                Debug.WriteLine($"*** {sourceFilePath} copied to {folderPath}");
                configExists = true;
                return configExists;
            }
            // If not we will create our own 
            Debug.WriteLine($"*** No {fileName} exists neither in {folderPath} nor in {sourceFilePath}");
            Debug.WriteLine("*** You need to create one, either from code or manually"); 
            return false;
        }

        /// <summary>
        /// Write IConfiguration object to Debug. 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="parentKey"></param>
        public static void WriteConfiguration(IConfiguration config, string parentKey = "")
        {
            Debug.WriteLine("*** Write out configuration ***");
            foreach (var section in config.GetChildren())
            {
                string key = parentKey == "" ? section.Key : $"{parentKey}:{section.Key}";

                if (section.GetChildren().Any())
                {
                    WriteConfiguration(section, key);
                }
                else
                {
                    Debug.WriteLine($"*** {key}: {section.Value}");
                }
            }
        }

    }
}
