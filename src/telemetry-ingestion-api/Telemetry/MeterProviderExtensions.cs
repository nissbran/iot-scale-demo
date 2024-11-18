using OpenTelemetry.Metrics;

namespace TelemetryIngestionApi.Telemetry;

public static class MeterProviderExtensions
{
    internal static MeterProviderBuilder AddConsumedEventsMetrics(this MeterProviderBuilder builder)
    {
        builder.AddMeter(TelemetryEventConsumedMetrics.InstrumentationName);
        return builder.AddInstrumentation(provider => provider.GetRequiredService<TelemetryEventConsumedMetrics>());
    }
}