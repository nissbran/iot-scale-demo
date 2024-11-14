using System.Diagnostics.Metrics;

namespace MessageMediator.Telemetry;

public class EventConsumedMetrics : IDisposable
{
    internal static readonly string InstrumentationName = "MessageRouter.EventConsumed";
    internal static readonly string InstrumentationVersion = "0.1";

    private readonly Meter _meter;
    private readonly Counter<long> _messagesCounter;
    private readonly ObservableGauge<long> _messagesGauge;
    private readonly Counter<long> _deviceRegisteredCounter;
    private readonly ObservableGauge<long> _deviceRegisteredGauge;
    private readonly Counter<long> _alertsRaisedCounter;
    private readonly ObservableGauge<long> _alertsRaisedGauge;
    private readonly Counter<long> _temperatureTelemetryCounter;
    private readonly ObservableGauge<long> _temperatureTelemetryGauge;
    private readonly Histogram<long> _temperatureValueHistogram;
    private long _messages;
    private long _devicesRegistered;
    private long _alertsRaised;
    private long _temperatureTelemetryCount;
    
    
    public EventConsumedMetrics()
    {
        _meter = new Meter(InstrumentationName, InstrumentationVersion);
        
        _messagesCounter = _meter.CreateCounter<long>("events.messages.consumed");
        _messagesGauge = _meter.CreateObservableGauge("events.messages.consumed",
            () => Interlocked.Exchange(ref _messages, 0));
        _deviceRegisteredCounter = _meter.CreateCounter<long>("events.device.registered");
        _deviceRegisteredGauge = _meter.CreateObservableGauge("events.device.registered",
            () => Interlocked.Exchange(ref _devicesRegistered, 0));
        _alertsRaisedCounter = _meter.CreateCounter<long>("events.alerts.raised");
        _alertsRaisedGauge = _meter.CreateObservableGauge("events.alerts.raised",
            () => Interlocked.Exchange(ref _alertsRaised, 0));
        _temperatureTelemetryCounter = _meter.CreateCounter<long>("events.temperature.telemetry");
        _temperatureTelemetryGauge = _meter.CreateObservableGauge("events.temperature.telemetry",
            () => Interlocked.Exchange(ref _temperatureTelemetryCount, 0));
        _temperatureValueHistogram = _meter.CreateHistogram<long>("events.temperature.value.histogram", "Celsius", "Temperature values");
    }
    
    public void IncrementMessages()
    {
        _messagesCounter.Add(1);
        Interlocked.Increment(ref _messages);
    }
    
    public void IncrementDeviceRegistered()
    {
        _deviceRegisteredCounter.Add(1);
        Interlocked.Increment(ref _devicesRegistered);
    }
    
    public void IncrementAlertsRaised()
    {
        _alertsRaisedCounter.Add(1);
        Interlocked.Increment(ref _alertsRaised);
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