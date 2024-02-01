using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TemperatureDevice;

public class TemperatureIotHubDeviceSimulator : IHostedService
{
    private readonly ILogger<TemperatureIotHubDeviceSimulator> _logger;
    private readonly DeviceRegistrar _deviceRegistrar;
    private readonly int _numberOfDevices;
    private readonly string? _devicePrefix;
    private readonly Task[] _tasks;
    private readonly ConcurrentDictionary<string, TemperatureIotHubDevice> _devices = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();


    public TemperatureIotHubDeviceSimulator(ILogger<TemperatureIotHubDeviceSimulator> logger, DeviceRegistrar deviceRegistrar, IConfiguration configuration)
    {
        _logger = logger;
        _deviceRegistrar = deviceRegistrar;
        _numberOfDevices = configuration.GetValue<int>("NumberOfDevices");
        _tasks = new Task[_numberOfDevices];
        _devicePrefix = configuration["DevicePrefix"];
    }

    public Task StartAsync(CancellationToken cancellationToken)
    { 
        _logger.LogInformation("Temperature Device simulator started at: {time:s}", DateTimeOffset.Now);
        
        _logger.LogInformation("Registering {NumberOfDevices} devices...", _numberOfDevices);
        
        for (var i = 0; i < _numberOfDevices; i++)
        {
            var deviceNumber = i;
            _tasks[i] = Task.Run(async () =>
            {
                var device = await _deviceRegistrar.RegisterDeviceAsync($"client_{_devicePrefix}_{deviceNumber + 1:000}", _cancellationTokenSource.Token);
                _devices.TryAdd(device.DeviceId, device);
                await device.SendMessagesAsync(_cancellationTokenSource.Token);
            }, cancellationToken);
        }
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Devices...");
        
        _logger.LogInformation("Current number of devices: {NumberOfDevices}", _devices.Count);
        
        var deviceList = _devices.Values.ToList();
        await Parallel.ForEachAsync(deviceList, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (device, token) =>
        {
            await device.DisconnectAsync();
        });
        await Task.Delay(1000, cancellationToken);
        _cancellationTokenSource.Cancel();
        _logger.LogInformation("Temperature Device simulator stopped at: {time:s}", DateTimeOffset.Now);
    }
}