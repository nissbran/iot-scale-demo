using Dapr.Client;
using MessageRouter.Contract;
using MessageRouter.Telemetry;

namespace MessageRouter.MessageHandlers;

public class AlertRouter(DaprClient daprClient, EventConsumedMetrics eventConsumedMetrics, ILogger<DeviceRegistrationHandler> logger) : MessageHandler<TemperatureTooHighAlert>
{
    public override async ValueTask<MessageHandlingResult> Handle(TemperatureTooHighAlert message)
    {
        try
        {
            await daprClient.PublishEventAsync("commands", "commands", new IncreaseCooling(message.DeviceId));
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