using MediatR;
using MessageMediator.Contract;
using MessageMediator.Telemetry;

namespace MessageMediator.RequestHandlers;

public class DeviceConnectionStateHandler(IotHubConnectionMetrics iotHubConnectionMetrics, ILogger<DeviceConnectionStateHandler> logger) : IRequestHandler<DeviceConnected,MessageHandlingResult>
{
    public Task<MessageHandlingResult> Handle(DeviceConnected request, CancellationToken cancellationToken)
    {
        iotHubConnectionMetrics.IncrementConnected();
        logger.LogInformation("Device connected {DeviceId}", request.DeviceId);
        return Task.FromResult(new MessageHandlingResult(true));
    }
}

public class DeviceDisconnectedStateHandler(IotHubConnectionMetrics iotHubConnectionMetrics, ILogger<DeviceDisconnectedStateHandler> logger) : IRequestHandler<DeviceDisconnected,MessageHandlingResult>
{
    public Task<MessageHandlingResult> Handle(DeviceDisconnected request, CancellationToken cancellationToken)
    {
        iotHubConnectionMetrics.IncrementDisconnected();
        logger.LogInformation("Device disconnected {DeviceId}", request.DeviceId);
        return Task.FromResult(new MessageHandlingResult(true));
    }
}