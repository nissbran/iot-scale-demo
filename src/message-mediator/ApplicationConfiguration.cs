using MessageMediator.Services;
using MessageMediator.Services.Kafka;
using MessageMediator.Telemetry;

namespace MessageMediator;

internal static class ApplicationConfiguration
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddDaprClient();
        builder.Services.AddSingleton<EventConsumedMetrics>();
        builder.Services.AddSingleton<IotHubConnectionMetrics>();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();
        });

        builder.Services.AddSingleton<IKafkaConnectionProvider, EventHubKafkaConnectionProvider>();
        builder.Services.AddHostedService<ConsumerService>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseHealthChecks("/healthz");
        if (ObservabilityConfiguration.UsePrometheusEndpoint)
        {
            app.MapPrometheusScrapingEndpoint().RequireHost("*:9090");
        }
        
        return app;
    }
}
