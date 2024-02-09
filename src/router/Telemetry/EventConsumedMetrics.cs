using System.Diagnostics.Metrics;
using System.Net;

namespace MessageRouter.Telemetry;

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
    private long _messages;
    private long _devicesRegistered;
    private long _alertsRaised;
    
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
    
    public void Dispose()
    {
        _meter.Dispose();
    }
}