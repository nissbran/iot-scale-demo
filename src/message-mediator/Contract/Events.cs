using MediatR;

namespace MessageMediator.Contract;


public record IotMessage(string DeviceId, string Type, string Source, string Payload, string HubName, string OpType);

public abstract class DeviceToCloudMessage
{
    public string DeviceId { get; set; }
}

public abstract class TelemetryMessage : DeviceToCloudMessage
{
}

public abstract class CommandMessage : DeviceToCloudMessage
{
}

public class RegisterDevice : CommandMessage, IRequest<MessageHandlingResult>
{
    public string AssignedHub { get; init; }
    public string Location { get; init; }
    public string DeviceType { get; init; }
}

public class TemperatureTelemetry : TelemetryMessage, INotification
{
    public decimal Temperature { get; init; }
}

public class TemperatureTooHighAlert : CommandMessage, IRequest<MessageHandlingResult>
{
    public decimal Threshold { get; init; }
    public decimal Temperature { get; init; }
}

public class DeviceConnected : CommandMessage, IRequest<MessageHandlingResult>
{
    public string SequenceNumber { get; init; }
}

public class DeviceDisconnected : CommandMessage, IRequest<MessageHandlingResult>
{
    public string SequenceNumber { get; init; }
}

public class MessageHandlingResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    public MessageHandlingResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
}