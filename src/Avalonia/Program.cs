using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



namespace MyMvvmApp;

public static class Program
{
    private static IHost _host = null!;

    internal static IServiceProvider? ServiceProvider
        => !Design.IsDesignMode
            ? _host.Services
            : null;
    //throw new InvalidOperationException("ServiceProvider is not available in design mode.")    ;



    [STAThread]
    public static async Task Main(string[] args)
    {
        try
        {
            _host = StartUp.BuildHost(args);
            await _host.StartAsync();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            /*  When using the IHost approach, you shouldn't manually dispose of the ServiceProvider.
                Instead, you should dispose of the IHost itself.
                Cleanup on exit: This shuts down the DI container and disposes of all services
            */
            _host.StopAsync().GetAwaiter().GetResult();
        }
    }



    private static AppBuilder BuildAvaloniaApp()
        // Use the service provider to create App( IServiceProvider)
        => AppBuilder.Configure(() => ServiceProvider!.GetRequiredService<App>())
            .UsePlatformDetect()
            .WithInterFont() // Recommended for Avalonia 11
            .LogToTrace();
}