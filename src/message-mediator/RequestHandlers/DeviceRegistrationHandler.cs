using Dapr.Client;
using MediatR;
using MessageMediator.Contract;
using MessageMediator.Telemetry;

namespace MessageMediator.RequestHandlers;

public class DeviceRegistrationHandler : IRequestHandler<RegisterDevice,MessageHandlingResult>
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

    public async Task<MessageHandlingResult> Handle(RegisterDevice request, CancellationToken cancellationToken)
    {
        _eventConsumedMetrics.IncrementDeviceRegistered();
        _logger.LogInformation("Device registered {DeviceId}", request.DeviceId);
        try
        {
            await _daprClient.SaveStateAsync("device-catalog", request.DeviceId, new DeviceRegistrationState
            {
                DeviceId = request.DeviceId,
                AssignedHub = request.AssignedHub,
                Location = request.Location,
                DeviceType = request.DeviceType
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving device registration state");
            return new MessageHandlingResult(false, e.Message);
        }
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