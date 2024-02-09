using MessageRouter.Contract;
using MessageRouter.Telemetry;

namespace MessageRouter.MessageHandlers;

public class DeviceConnectionStateHandler(IotHubConnectionMetrics iotHubConnectionMetrics, ILogger<DeviceRegistrationHandler> logger) : MessageHandler<DeviceConnected>
{
    public override ValueTask<MessageHandlingResult> Handle(DeviceConnected message)
    {
        iotHubConnectionMetrics.IncrementConnected();
        logger.LogInformation("Device connected {DeviceId}", message.DeviceId);
        return ValueTask.FromResult(new MessageHandlingResult(true));
    }
}

public class DeviceDisconnectedStateHandler(IotHubConnectionMetrics iotHubConnectionMetrics, ILogger<DeviceRegistrationHandler> logger) : MessageHandler<DeviceDisconnected>
{
    public override ValueTask<MessageHandlingResult> Handle(DeviceDisconnected message)
    {
        iotHubConnectionMetrics.IncrementDisconnected();
        logger.LogInformation("Device disconnected {DeviceId}", message.DeviceId);
        return ValueTask.FromResult(new MessageHandlingResult(true));
    }
}