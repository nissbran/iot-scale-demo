namespace TemperatureDevice.Messages;

public record RegisterDevice(string DeviceId, string AssignedHub, string Location, string DeviceType);
public record TemperatureTelemetry(string DeviceId, decimal Temperature);
public record TemperatureTooHighAlert(string DeviceId, decimal Threshold, decimal Temperature);
