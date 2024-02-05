using MessageRouter.Contract;

namespace MessageRouter.MessageHandlers;

public interface IMessageHandler
{
    Type MessageType { get; }
        
    ValueTask<MessageHandlingResult> Handle(DeviceToCloudMessage message);
}

public interface IMessageHandler<in T> : IMessageHandler
    where T : DeviceToCloudMessage
{
    ValueTask<MessageHandlingResult> Handle(T message);
}

public abstract class MessageHandler<T> : IMessageHandler<T>
    where T : DeviceToCloudMessage
{
    public Type MessageType { get; } = typeof(T);

    public async ValueTask<MessageHandlingResult> Handle(DeviceToCloudMessage message)
    {
        return await Handle((T)message);
    }

    public abstract ValueTask<MessageHandlingResult> Handle(T message);
}

public record MessageHandlingResult(bool Success, string? ErrorMessage = null);