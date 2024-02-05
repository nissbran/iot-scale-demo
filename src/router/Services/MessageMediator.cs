using MessageRouter.Contract;
using MessageRouter.MessageHandlers;

namespace MessageRouter.Services;

public class MessageMediator
{
    private readonly ILogger<MessageMediator> _logger;
    private readonly IDictionary<Type, IMessageHandler> _messageHandlers;

    public MessageMediator(IEnumerable<IMessageHandler> messageHandlers, ILogger<MessageMediator> logger)
    {
        _logger = logger;
        _messageHandlers = messageHandlers.ToDictionary(handler => handler.MessageType);
    }
    
    public async ValueTask RouteMessage(DeviceToCloudMessage message)
    {
        switch (message)
        {
            case TelemetryMessage telemetryMessage:
               // _logger.LogInformation("Telemetry message received from {DeviceId}", telemetryMessage.DeviceId);
                break;
            case CommandMessage commandMessage:
                await GetHandler(commandMessage.GetType()).Handle(message);
                break;
            default:
                _logger.LogWarning("Unknown message type {MessageType}", message.GetType().Name);
                break;
        }
    }
        
    private IMessageHandler GetHandler(Type message)
    {
        if (!_messageHandlers.TryGetValue(message, out var commandHandler))
            throw new ArgumentException($"Message handler for {message.Name} could not be found.");

        return commandHandler;
    }
}