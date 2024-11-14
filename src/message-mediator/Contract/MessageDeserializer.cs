using System.Text.Json;

namespace MessageMediator.Contract;

public static class MessageDeserializer
{
    public static DeviceToCloudMessage? Deserialize(IotMessage message)
    {
        return message.Source switch
        {
            "Telemetry" => message.Type switch
            {
                "TemperatureTelemetry" => JsonSerializer.Deserialize<TemperatureTelemetry>(message.Payload),
                "RegisterDevice" => JsonSerializer.Deserialize<RegisterDevice>(message.Payload),
                "TemperatureTooHighAlert" => JsonSerializer.Deserialize<TemperatureTooHighAlert>(message.Payload),
                _ => null
            },
            "deviceConnectionStateEvents" => DeserializeConnectionStateMessage(message),
            _ => null
        };
    }

    private static DeviceToCloudMessage? DeserializeConnectionStateMessage(IotMessage message)
    {
        DeviceToCloudMessage? connectionStateMessage = message.OpType switch
        {
            "deviceConnected" => JsonSerializer.Deserialize<DeviceConnected>(message.Payload),
            "deviceDisconnected" => JsonSerializer.Deserialize<DeviceDisconnected>(message.Payload),
            _ => null
        };
        if (connectionStateMessage != null)
        {
            connectionStateMessage.DeviceId = message.DeviceId;
        }
        return connectionStateMessage;
    }
}
    