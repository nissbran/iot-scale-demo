using Microsoft.AspNetCore.Mvc;
using Serilog;
using YarpHubProxy.Routing;
using YarpHubProxy.Services;
using YarpHubProxy.Telemetry;

namespace YarpHubProxy;

internal static class ApplicationConfiguration
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddDaprClient();
        builder.Services.AddSingleton<CommandsSentMetrics>();
        builder.Services.AddSingleton<QueriesMetrics>();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.UseServiceDiscovery();
        });
        builder.Services.AddIotHubProxy();
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

        app.UseSerilogRequestLogging();
        
        app.MapPost("commands/devices/{deviceId}/send-command", async (string deviceId, IotHubSender iotHubSender, [FromBody]string message) =>
        {
            await iotHubSender.SendCommandAsync(deviceId, message);
            return TypedResults.Ok();
        });
        app.MapReverseProxy(proxyPipeline  =>
        {
            //proxyPipeline.UseLoadBalancing();
        });

        return app;
    }
}
