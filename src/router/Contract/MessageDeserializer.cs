using System.Text.Json;

namespace MessageRouter.Contract;

public static class MessageDeserializer
{
    public static DeviceToCloudMessage? Deserialize(IotMessage message)
    {
        return message.Type switch
        {
            "TemperatureTelemetry" => JsonSerializer.Deserialize<TemperatureTelemetry>(message.Payload),
            "RegisterDevice" => JsonSerializer.Deserialize<RegisterDevice>(message.Payload),
            "TemperatureTooHighAlert" => JsonSerializer.Deserialize<TemperatureTooHighAlert>(message.Payload),
            _ => null
        };
    }
}