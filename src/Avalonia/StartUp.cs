using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMvvmApp.ViewModels;



namespace MyMvvmApp;

internal class StartUp
{
    internal static IHost BuildHost(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<App>();
        
        //
        // Init Configuration
        //
        builder.Configuration.Sources.Clear();
        IHostEnvironment env = builder.Environment;
        builder.Configuration
            .AddJsonFile("appsettings.json", false, true)
            //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
            //.AddJsonFile(AppSettingsExtensions.UserConfigFileName, true, true)
            .AddCommandLine(args);

        //
        // Init Services
        //
        // IOptions Pattern
        // Register app settings and use as IOptions<AppSettings>
        // either by constructor injection or by 
        // var x =_host.builder.Services.GetService <IOptions<AppSettings>>();
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings")!);

        // ViewModels
        builder.Services.AddSingleton<MainViewModel>();

        //
        // Init Application
        //
        return builder.Build();
    }
}