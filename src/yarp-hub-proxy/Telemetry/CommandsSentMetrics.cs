﻿using System.Diagnostics.Metrics;

namespace YarpHubProxy.Telemetry;

public class CommandsSentMetrics : IDisposable
{
    internal static readonly string InstrumentationName = "YarpProxy.CommandsSent";
    internal static readonly string InstrumentationVersion = "0.1";

    private readonly Meter _meter;
    private readonly Counter<long> _commandSentCounter;
    private readonly ObservableGauge<long> _commandSentGauge;
    private long _commandSent;
    
    public CommandsSentMetrics()
    {
        _meter = new Meter(InstrumentationName, InstrumentationVersion);

        _commandSentCounter = _meter.CreateCounter<long>("commands.sent");
        _commandSentGauge = _meter.CreateObservableGauge("commands.sent",
            () => Interlocked.Exchange(ref _commandSent, 0));
    }
    
    public void IncrementCommandSent()
    {
        _commandSentCounter.Add(1);
        Interlocked.Increment(ref _commandSent);
    }
    
    
    public void Dispose()
    {
        _meter.Dispose();
    }
}