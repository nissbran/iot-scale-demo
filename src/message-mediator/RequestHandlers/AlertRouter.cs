using Dapr.Client;
using MediatR;
using MessageMediator.Contract;
using MessageMediator.Telemetry;

namespace MessageMediator.RequestHandlers;

public class AlertRouter(DaprClient daprClient, EventConsumedMetrics eventConsumedMetrics, ILogger<DeviceRegistrationHandler> logger) : IRequestHandler<TemperatureTooHighAlert, MessageHandlingResult>
{
    public async Task<MessageHandlingResult> Handle(TemperatureTooHighAlert message, CancellationToken cancellationToken)
    {
        try
        {
            await daprClient.PublishEventAsync("commands", "commands", new IncreaseCooling(message.DeviceId));
            logger.LogInformation("Temperature Too High Alert raised for device {DeviceId}, sent increase cooling command", message.DeviceId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error publishing command");
            return new MessageHandlingResult(false, e.Message);
        }
        eventConsumedMetrics.IncrementAlertsRaised();
        return new MessageHandlingResult(true);
    }
}

public record IncreaseCooling(string DeviceId);