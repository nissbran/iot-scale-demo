using OpenTelemetry.Metrics;

namespace YarpHubProxy.Telemetry;

public static class MeterProviderExtensions
{
    internal static MeterProviderBuilder AddConsumedEventsMetrics(this MeterProviderBuilder builder)
    {
        builder.AddMeter(CommandsSentMetrics.InstrumentationName);
        return builder.AddInstrumentation(provider => provider.GetRequiredService<CommandsSentMetrics>());
    }
    
    internal static MeterProviderBuilder AddQueriesMetrics(this MeterProviderBuilder builder)
    {
        builder.AddMeter(QueriesMetrics.InstrumentationName);
        return builder.AddInstrumentation(provider => provider.GetRequiredService<QueriesMetrics>());
    }
}