using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

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

        public static string ChangeString2FolderName(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            // Convert to lowercase (important for Android)
            string folderName = input.ToLowerInvariant();

            // Replace spaces with underscores
            folderName = folderName.Replace(" ", "_");

            folderName = Regex.Replace(folderName, @"æ", "ae");
            folderName = Regex.Replace(folderName, @"ø", "oe");
            folderName = Regex.Replace(folderName, @"å", "aa");

            // Remove all special characters, only allowing letters, digits, underscores, and hyphens
            folderName = Regex.Replace(folderName, @"[^a-z0-9_\-]", "");

            // Trim any remaining whitespace (although we've replaced spaces, it's a good safeguard)
            folderName = folderName.Trim();

            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentException($"After cleanup of string = {input} became empty and that is not allowed");
            }

            // Ensure the folder name is not too long (optional, but Android has a limit of 255 bytes)
            if (folderName.Length > 100)
            {
                folderName = folderName.Substring(0, 100);
            }

            return folderName;
        }

        /// <summary>
        /// Strips non-ASCII characters, non-visible characters, and whitespace from the input string.
        /// </summary>
        /// <param name="input">The string to clean.</param>
        /// <returns>The cleaned string.</returns>
        public static string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input; // Return empty or null strings as-is.
            }

            // Regular expression to match any character that is:
            // 1. Not ASCII (outside the range of characters 0x20 to 0x7E, excluding control characters)
            // 2. White space (\s)
            // 3. Non-visible characters
            string pattern = @"[^\x20-\x7E]|\s";

            // Replace matches of the regex with an empty string.
            string cleaned = Regex.Replace(input, pattern, "");

            return cleaned;
        }

        public static void WriteObjectProperties<T>(T obj)
        {
            if (obj == null)
            {
                Console.WriteLine("Object is null.");
                return;
            }

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();

            Console.WriteLine($"Properties of object of type {type.Name}:");

            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                object? value = null;
                try
                {
                    value = property.GetValue(obj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading property '{name}': {ex.Message}");
                    continue;
                }
                Console.WriteLine($"{name}: {value}");
            }

        }

        // Deep copy method that copies properties from one object to another
        public static void DeepCopyObjectTo<T>(this T source, T destination)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException("Source or/and destination objects cannot be null");

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    object? value = property.GetValue(source, null);
                    property.SetValue(destination, value, null);
                }
            }
        }
    }
}
