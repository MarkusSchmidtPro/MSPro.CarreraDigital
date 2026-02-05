using System;
using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;



namespace MyMvvmApp;

/// <summary>
///     Application config based on IOptions pattern.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0" />
/// <see
///     href="https://stackoverflow.com/questions/76818314/getting-an-appsettings-json-config-value-inside-program-cs-with-type-safety" />
public static class AppSettingsExtensions
{

    public static string UserConfigFileName =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "appsettings.json");



    extension(AppSettings appSettings)
    {
        /// <summary>
        ///     Save the current configuration instance to user config files.
        /// </summary>
        /// <see cref="UserConfigFileName" />
        public void Save()
        {
            string j = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
            File.WriteAllText(UserConfigFileName, j);
        }
    }
}