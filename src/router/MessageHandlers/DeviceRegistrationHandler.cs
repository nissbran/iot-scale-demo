using Dapr.Client;
using MessageRouter.Contract;
using MessageRouter.Telemetry;

namespace MessageRouter.MessageHandlers;

public class DeviceRegistrationHandler : MessageHandler<RegisterDevice>
{
    private readonly DaprClient _daprClient;
    private readonly EventConsumedMetrics _eventConsumedMetrics;
    private readonly ILogger<DeviceRegistrationHandler> _logger;

    public DeviceRegistrationHandler(DaprClient daprClient, EventConsumedMetrics eventConsumedMetrics, ILogger<DeviceRegistrationHandler> logger)
    {
        _daprClient = daprClient;
        _eventConsumedMetrics = eventConsumedMetrics;
        _logger = logger;
    }
    
    public override async ValueTask<MessageHandlingResult> Handle(RegisterDevice message)
    {
        _eventConsumedMetrics.IncrementDeviceRegistered();
        _logger.LogInformation("Device registered {DeviceId}", message.DeviceId);
        await _daprClient.SaveStateAsync("device-catalog", message.DeviceId, new DeviceRegistrationState
        {
            DeviceId = message.DeviceId,
            AssignedHub = message.AssignedHub,
            Location = message.Location,
            DeviceType = message.DeviceType
        });
        return new MessageHandlingResult(true);
    }
}

public class DeviceRegistrationState
{   
    public string DeviceId { get; set; }
    public string AssignedHub { get; set; }
    public string Location { get; set; }
    public string DeviceType { get; set; }
}