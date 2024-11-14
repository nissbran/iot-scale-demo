using System.Diagnostics.Metrics;

namespace MessageMediator.Telemetry;

public class IotHubConnectionMetrics : IDisposable
{
    internal static readonly string InstrumentationName = "MessageRouter.IotHubConnections";
    internal static readonly string InstrumentationVersion = "0.1";

    private readonly Meter _meter;
    private readonly Counter<long> _devicesConnectedCounter;
    private readonly ObservableGauge<long> _devicesConnectedGauge;
    private readonly Counter<long> _devicesDisconnectedCounter;
    private readonly ObservableGauge<long> _devicesDisconnectedGauge;
    
    private long _devicesConnected;
    private long _devicesDisconnected;

    public IotHubConnectionMetrics()
    {
        _meter = new Meter(InstrumentationName, InstrumentationVersion);

        _devicesConnectedCounter = _meter.CreateCounter<long>("events.devices.connected");
        _devicesConnectedGauge = _meter.CreateObservableGauge("events.devices.connected",
            () => Interlocked.Exchange(ref _devicesConnected, 0));
        _devicesDisconnectedCounter = _meter.CreateCounter<long>("events.devices.disconnected");
        _devicesDisconnectedGauge = _meter.CreateObservableGauge("events.devices.disconnected",
            () => Interlocked.Exchange(ref _devicesConnected, 0));
    }

    public void IncrementConnected()
    {
        _devicesConnectedCounter.Add(1);
        Interlocked.Increment(ref _devicesConnected);
    }
    
    public void IncrementDisconnected()
    {
        _devicesDisconnectedCounter.Add(1);
        Interlocked.Increment(ref _devicesDisconnected);
    }
    
    public void Dispose()
    {
        _meter.Dispose();
    }
}

