using MediatR;
using MessageMediator.Contract;
using MessageMediator.Telemetry;

namespace MessageMediator.Notifications;

public class TelemetryLoggingHandler(EventConsumedMetrics metrics, ILogger<TelemetryLoggingHandler> logger) : INotificationHandler<TemperatureTelemetry>
{
    public Task Handle(TemperatureTelemetry notification, CancellationToken cancellationToken)
    {
        //logger.LogInformation("Temperature telemetry received {DeviceId} {Temperature}", notification.DeviceId, notification.Temperature);
        metrics.RecordTemperatureTelemetryValue((long)notification.Temperature);
        return Task.CompletedTask;
    }
}