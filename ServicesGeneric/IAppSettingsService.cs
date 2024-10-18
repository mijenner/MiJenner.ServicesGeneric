using Microsoft.Extensions.Configuration;

namespace MiJenner.ServicesGeneric
{
    /// <summary>
    /// IAppSettingsService is for application settings, often set once and read infrequently. 
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// Returns a IConfiguration object. 
        /// </summary>
        /// <returns></returns>
        IConfiguration GetConfiguration();
        /// <summary>
        /// ConfigExists is true if an appsettings.json file exists. 
        /// else false. 
        /// </summary>
        bool ConfigExists { get;  }
        /// <summary>
        /// Returns full path to and including "appsettings.json" (or what you decide to call it)
        /// </summary>
        /// <returns></returns>
        string GetFilePath(); 
        /// <summary>
        /// Returns full folder path to "appsettings.json" (or what you decide to call it) 
        /// </summary>
        /// <returns></returns>
        string GetFolderPath(); // Get the folder path
        /// <summary>
        /// Allows user to write an "appsettings.json" file from the app. 
        /// </summary>
        void CreateSettingsFile(object aSettingsObject); 
    }
}
