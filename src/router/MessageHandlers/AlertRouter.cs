using Dapr.Client;
using MessageRouter.Contract;
using MessageRouter.Telemetry;

namespace MessageRouter.MessageHandlers;

public class AlertRouter : MessageHandler<TemperatureTooHighAlert>
{
    private readonly DaprClient _daprClient;
    private readonly EventConsumedMetrics _eventConsumedMetrics;
    private readonly ILogger<DeviceRegistrationHandler> _logger;

    public AlertRouter(DaprClient daprClient, EventConsumedMetrics eventConsumedMetrics, ILogger<DeviceRegistrationHandler> logger)
    {
        _daprClient = daprClient;
        _eventConsumedMetrics = eventConsumedMetrics;
        _logger = logger;
    }

    public override async ValueTask<MessageHandlingResult> Handle(TemperatureTooHighAlert message)
    {
        await _daprClient.PublishEventAsync("commands", "commands", new IncreaseCooling(message.DeviceId));
        _eventConsumedMetrics.IncrementAlertsRaised();
        return new MessageHandlingResult(true);
    }
}

public record IncreaseCooling(string DeviceId);