using CommandSender.Contract;
using CommandSender.Services;
using CommandSender.Telemetry;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace CommandSender;

internal static class ApplicationConfiguration
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddDaprClient();
        builder.Services.AddSingleton<CommandsSentMetrics>();
        builder.Services.AddServiceDiscovery();
        
        builder.Services.AddSingleton<IotHubSender>();
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseCloudEvents();
        app.MapSubscribeHandler();
        app.UseHealthChecks("/healthz");
        if (ObservabilityConfiguration.UsePrometheusEndpoint)
        {
            app.MapPrometheusScrapingEndpoint().RequireHost("*:9090");
        }

        app.MapPost("handler/cooling/increase", [Topic("commands", "commands")] async (IotHubSender iotHubSender, [FromBody]IncreaseCooling increaseCooling) =>
        {
            await iotHubSender.SendCommandAsync(increaseCooling.DeviceId, new IncreaseCoolingCommand(increaseCooling.DeviceId));
            return TypedResults.Ok();
        });
        
        return app;
    }
}
