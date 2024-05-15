using System.Diagnostics.Metrics;

namespace YarpHubProxy.Telemetry;

public class QueriesMetrics : IDisposable
{
    internal static readonly string InstrumentationName = "YarpProxy.Queries";
    internal static readonly string InstrumentationVersion = "0.1";

    private readonly Meter _meter;
    private readonly Counter<long> _queriesRoutedCounter;
    private readonly ObservableGauge<long> _queriesRoutedGauge;
    private long _queriesRouted;

    public QueriesMetrics()
    {
        _meter = new Meter(InstrumentationName, InstrumentationVersion);

        _queriesRoutedCounter = _meter.CreateCounter<long>("queries.routed");
        _queriesRoutedGauge = _meter.CreateObservableGauge("queries.routed",
            () => Interlocked.Exchange(ref _queriesRouted, 0));
    }

    public void IncrementQueriesRouted()
    {
        _queriesRoutedCounter.Add(1);
        Interlocked.Increment(ref _queriesRouted);
    }


    public void Dispose()
    {
        _meter.Dispose();
    }
}
