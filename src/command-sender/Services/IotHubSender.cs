using System.Text;
using System.Text.Json;
using CommandSender.Contract;
using CommandSender.Telemetry;
using Dapr.Client;
using Microsoft.Azure.Devices;

namespace CommandSender.Services;

public class IotHubSender
{
    private readonly DaprClient _client;
    private readonly CommandsSentMetrics _commandsSentMetrics;
    private readonly Dictionary<string, ServiceClient> _iotHubs;
    
    public IotHubSender(IConfiguration configuration, DaprClient client, CommandsSentMetrics commandsSentMetrics)
    {
        _client = client;
        _commandsSentMetrics = commandsSentMetrics;
        _iotHubs = new Dictionary<string, ServiceClient>
        {
            { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]).HostName, ServiceClient.CreateFromConnectionString(configuration["IotHubs:IotHubA"]) },
            { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubB"]).HostName, ServiceClient.CreateFromConnectionString(configuration["IotHubs:IotHubB"]) }
        };
    }
    
    public async ValueTask SendCommandAsync(string deviceId, Command command)
    {
        var registrationState = await _client.GetStateAsync<DeviceRegistrationState>("device-catalog", deviceId);
        
        using var message = new Message(JsonSerializer.SerializeToUtf8Bytes(command));
        message.Ack = DeliveryAcknowledgement.Full;
        message.Properties["command-name"] = command.Name;
        var hubClient = _iotHubs[registrationState.AssignedHub];
        await hubClient.SendAsync(deviceId, message);
        _commandsSentMetrics.IncrementCommandSent();
    }
    
}

public class DeviceRegistrationState
{   
    public string DeviceId { get; set; }
    public string AssignedHub { get; set; }
}