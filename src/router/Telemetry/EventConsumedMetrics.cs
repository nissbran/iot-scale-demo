using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace MessageRouter.Telemetry;

public class EventConsumedMetrics : IDisposable
{
    internal static readonly string InstrumentationName = "MessageRouter.EventConsumed";
    internal static readonly string InstrumentationVersion = "0.1";

    private readonly Meter _meter;
    private readonly Counter<long> _messagesCounter;
    private readonly ObservableGauge<long> _messagesGauge;
    private long _messages;
    
    public EventConsumedMetrics()
    {
        _meter = new Meter(InstrumentationName, InstrumentationVersion);
        
        _messagesCounter = _meter.CreateCounter<long>("events.messages.consumed");
        _messagesGauge = _meter.CreateObservableGauge("events.messages.consumed",
            () => Interlocked.Exchange(ref _messages, 0));
    }
    
    public void IncrementMessages()
    {
        _messagesCounter.Add(1);
        Interlocked.Increment(ref _messages);
    }
    
    public void Dispose()
    {
        _meter.Dispose();
    }
}

public static class MeterProviderExtensions
{
    internal static MeterProviderBuilder AddConsumedEventsMetrics(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EventConsumedMetrics.InstrumentationName);
        return builder.AddInstrumentation(provider => provider.GetRequiredService<EventConsumedMetrics>());
    }
}
