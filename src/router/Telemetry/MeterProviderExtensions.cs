using OpenTelemetry.Metrics;

namespace MessageRouter.Telemetry;

public static class MeterProviderExtensions
{
    internal static MeterProviderBuilder AddConsumedEventsMetrics(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EventConsumedMetrics.InstrumentationName);
        return builder.AddInstrumentation(provider => provider.GetRequiredService<EventConsumedMetrics>());
    }
    
    internal static MeterProviderBuilder AddIotHubConnectionMetrics(this MeterProviderBuilder builder)
    {
        builder.AddMeter(IotHubConnectionMetrics.InstrumentationName);
        return builder.AddInstrumentation(provider => provider.GetRequiredService<IotHubConnectionMetrics>());
    }
}