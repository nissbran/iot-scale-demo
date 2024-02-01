using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Serilog;

namespace TemperatureDevice;

public class TemperatureIotHubDevice : IDisposable
{
    private readonly X509Certificate2 _deviceCertificate;
    private readonly DeviceAuthenticationWithX509Certificate _deviceAuthentication;
    private readonly DeviceClient _client;
    private readonly string _deviceId;
    private bool _stopping = false;
    
    public string DeviceId => _deviceId;

    public TemperatureIotHubDevice(X509Certificate2 deviceCertificate, string deviceId, string assignedHub)
    {
        _deviceCertificate = deviceCertificate;
        _deviceId = deviceId;
        _deviceAuthentication = new DeviceAuthenticationWithX509Certificate(deviceId, deviceCertificate);
        _client = DeviceClient.Create(assignedHub, _deviceAuthentication, TransportType.Mqtt);
    }
    
    public async Task SendMessagesAsync(CancellationToken stoppingToken)
    {
        while (!_stopping && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messageBody = $"{{\"deviceId\": \"{_deviceId}\", \"temperature\": {Random.Shared.Next(1, 60)}}}";
                using var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                await _client.SendEventAsync(message, stoppingToken);
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