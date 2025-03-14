using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TelemetryIngestionApi;
using TelemetryIngestionApi.Telemetry;

const string appName = "telemetry-ingestion-api";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Sixteen)
    .CreateBootstrapLogger();

Log.Information("Starting up {Application}", appName);

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile("appsettings.local.json", true);

    var app = builder
        .ConfigureTelemetry()
        .ConfigureServices()
        .ConfigurePipeline();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception when starting {Application}", appName);
}
finally
{
    Log.Information("Shut down complete for {Application}", appName);
    Log.CloseAndFlush();
}
