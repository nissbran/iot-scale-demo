using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using TemperatureDevice;

const string appName = "Temperature Device Runner";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
    .CreateBootstrapLogger();

Log.Information("Starting {Application}", appName);

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, configuration) => configuration.WriteTo.Console(theme: AnsiConsoleTheme.Sixteen))
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.local.json", true))
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<TemperatureIotHubDeviceSimulator>();
        services.AddSingleton<ClientCertificateGenerator>();
        services.AddSingleton<DeviceRegistrar>();
    })
    .UseConsoleLifetime()
    .Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception when starting {Application}", appName);
}
finally
{
    host.Dispose();
    Log.Information("Shut down complete for {Application}", appName);
    Log.CloseAndFlush();
    await Task.Delay(2000);
}