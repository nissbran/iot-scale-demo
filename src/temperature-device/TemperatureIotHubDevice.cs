using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;
using Serilog;
using TemperatureDevice.Messages;

namespace TemperatureDevice;

public class TemperatureIotHubDevice : IDisposable
{
    private readonly X509Certificate2 _deviceCertificate;
    private readonly DeviceAuthenticationWithX509Certificate _deviceAuthentication;
    private readonly DeviceClient _client;
    private readonly string _deviceId;
    private readonly string _assignedHub;
    private bool _stopping = false;
    
    public string DeviceId => _deviceId;

    public TemperatureIotHubDevice(X509Certificate2 deviceCertificate, string deviceId, string assignedHub)
    {
        _deviceCertificate = deviceCertificate;
        _deviceId = deviceId;
        _assignedHub = assignedHub;
        _deviceAuthentication = new DeviceAuthenticationWithX509Certificate(deviceId, deviceCertificate);
        _client = DeviceClient.Create(assignedHub, _deviceAuthentication, TransportType.Mqtt);
    }
    
    public async Task SendMessagesAsync(CancellationToken stoppingToken)
    {
        Log.Information("Device {DeviceId} registering", _deviceId);
        using var registerMessage = new Message(
            JsonSerializer.SerializeToUtf8Bytes(new RegisterDevice(_deviceId, _assignedHub, "NorthEurope", "TemperatureSensor")));
        registerMessage.Properties.Add("type", nameof(RegisterDevice));
        await _client.SendEventAsync(registerMessage, stoppingToken);
        
        while (!_stopping && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                var temperatureMessage = new TemperatureTelemetry(_deviceId, new Random().Next(10, 31));
                using var telemetryMessage = new Message(JsonSerializer.SerializeToUtf8Bytes(temperatureMessage));
                telemetryMessage.Properties.Add("type", nameof(TemperatureTelemetry));
                await _client.SendEventAsync(telemetryMessage, stoppingToken);

                if (temperatureMessage.Temperature >= 30)
                {
                    var alertMessage = new TemperatureTooHighAlert(_deviceId, 30, temperatureMessage.Temperature);
                    using var message = new Message(JsonSerializer.SerializeToUtf8Bytes(alertMessage));
                    message.Properties.Add("type", nameof(TemperatureTooHighAlert));
                    await _client.SendEventAsync(message, stoppingToken);
                }
                await Task.Delay(1000, stoppingToken);
            }
            catch (TaskCanceledException e)
            {
                Log.Warning(e, "Device {DeviceId} was canceled.", _deviceId);
            }
        }
        
        Log.Information("Device {DeviceId} stopped sending messages.", _deviceId);
    }

    public async Task StartSubscription(CancellationToken stoppingToken)
    {
        while (!_stopping && !stoppingToken.IsCancellationRequested)
        {
            var commands = await _client.ReceiveAsync(stoppingToken);
            
            Log.Information("Device {DeviceId} received command {CommandName}.", _deviceId, commands?.Properties["command-name"]);
                
            await _client.CompleteAsync(commands, stoppingToken);
        }
        
    }
    
    public async Task DisconnectAsync()
    {
        Log.Information("Disconnecting Device {DeviceId}.", _deviceId);
        _stopping = true;
        await _client.CloseAsync();
        Log.Information("Device {DeviceId} disconnected.", _deviceId);
    }

    public void Dispose()
    {
        _client.Dispose();
        _deviceAuthentication.Dispose();
        _deviceCertificate.Dispose();
    }
}