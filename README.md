## MiJenner Services Generic 
Offers a settingsmanager, a simple crud service and a few other helpers. 

## Settings manager 
To use the settings manager you create two objects, (a) one with properties representing settings for application and (b) one with properties representing settings for users. 

First, remember to add NuGet package and add: 
```cs 
using MiJenner.ServicesGeneric; 
```

You may want to use by example the following settings objects: 
```cs 
AppSettings appSettings = new AppSettings() { ConnString = "database.sqlite3", ApiString="anAPIstring", IsRunningOK = true };
UserSettings userSettings = new UserSettings() { UserName="Bent", WantsNotifications=false };
``` 

On desktop (Windows, macOS and Linux) you need to supply filename, application-name and company, aka organization name: 
```cs 
string SettingsFileName = "settings.json";
string AppName = "My App Name";
string CompanyName = "My Company Name";
FolderPolicy folderPolicy = FolderPolicy.AppDataLocal;
``` 

Next create an instance of SettingsManager as follows: 
```cs 
SettingsManager<AppSettings, UserSettings> settings = new SettingsManager<AppSettings, UserSettings>(SettingsFileName, AppName, CompanyName, folderPolicy, appSettings, userSettings);
```

Next call Initialize() on the instance to make software search for settings file. SettingsManager will first look in folder formed using AppName, CompanyName and folderPolicy, if found we are good to go. Next, it will look in the software installation location (copy-to-output) and if found it will copy to first-mentioned folder. Finally, if without luck in both locations it will create one from software, based on the objects fed into SettingsManager. 

```cs 
await settings.Initialize(); 
```

Now you can read and modify settings using normal "dot-notation", by example: 

```cs 
userSettings.UserName = "John Doe";
appSettings.ApiString = "Application Programming Interface"; 
``` 

You may want to save to disk, so the settings are available next time you run the application: 
```cs 
await settings.SaveSettingsToDiskAsync();
``` 

Note: Since we feed reference types into SettingsManager it doesn't matter if you refer to the settings directly, or via SettingsManager. In a Console application below will give the same output: 

```cs 
Console.WriteLine($"    - {nameof(appSettings.ConnString)}: {appSettings.ConnString}");
Console.WriteLine($"    - {nameof(settings.AppSettings.ConnString)}: {settings.AppSettings.ConnString}");
```

## CRUD service In-Memory 
Offers a simple CRUD service for quick and dirty testing of apps needing one. 

# Model 
Model must be simple, a list of objects. Two requirements: (a) Model must inherit from IHasGuid, and (b) it must have a property called Id of type Guid. Example: 

```cs
public class Item : IHasGuid 
{
   public Guid Id { get; set; }
   public required string Name { get; set; }
   public string? Description { get; set; }
}
```

# Interface
Interface: ICrudIdService 

# Implementation 
Implementation: CrudIdServiceInMemory 

# Example usage 
After having created a Model as shown above, then you can create a list of these objects using: 

```cs
ICrudIdService<Item> crudIdService = new CrudIdInMemory<Item>();
```
