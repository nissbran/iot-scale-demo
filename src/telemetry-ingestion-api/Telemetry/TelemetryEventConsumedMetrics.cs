using System.Diagnostics.Metrics;

namespace TelemetryIngestionApi.Telemetry;

public class TelemetryEventConsumedMetrics : IDisposable
{
    internal static readonly string InstrumentationName = "Telemetry.EventConsumed";
    internal static readonly string InstrumentationVersion = "0.1";

    private readonly Meter _meter;
    private readonly Counter<long> _temperatureTelemetryCounter;
    private readonly ObservableGauge<long> _temperatureTelemetryGauge;
    private readonly Histogram<long> _temperatureValueHistogram;
    private long _temperatureTelemetryCount;
    
    
    public TelemetryEventConsumedMetrics()
    {
        _meter = new Meter(InstrumentationName, InstrumentationVersion);
   
        _temperatureTelemetryCounter = _meter.CreateCounter<long>("events.temperature.telemetry");
        _temperatureTelemetryGauge = _meter.CreateObservableGauge("events.temperature.telemetry",
            () => Interlocked.Exchange(ref _temperatureTelemetryCount, 0));
        _temperatureValueHistogram = _meter.CreateHistogram<long>("events.temperature.value.histogram", "Celsius", "Temperature values");
    }
    
    public void RecordTemperatureTelemetryValue(long value)
    {
        _temperatureValueHistogram.Record(value);
        _temperatureTelemetryCounter.Add(1);
        Interlocked.Increment(ref _temperatureTelemetryCount);
    }
    
    public void Dispose()
    {
        _meter.Dispose();
    }
}