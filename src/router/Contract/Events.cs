namespace MessageRouter.Contract;


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

public class RegisterDevice : CommandMessage
{
    public string AssignedHub { get; init; }
    public string Location { get; init; }
    public string DeviceType { get; init; }
}

public class TemperatureTelemetry : TelemetryMessage
{
    public decimal Temperature { get; init; }
}

public class TemperatureTooHighAlert : CommandMessage
{
    public decimal Threshold { get; init; }
    public decimal Temperature { get; init; }
}

public class DeviceConnected : CommandMessage
{
    public string SequenceNumber { get; init; }
}

public class DeviceDisconnected : CommandMessage
{
    public string SequenceNumber { get; init; }
}