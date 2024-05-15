using System.Text;
using Dapr.Client;
using Microsoft.Azure.Devices;
using YarpHubProxy.Telemetry;

namespace YarpHubProxy.Services;

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
    
    public async ValueTask SendCommandAsync(string deviceId, string messageBody)
    {
        var registrationState = await _client.GetStateAsync<DeviceRegistrationState>("device-catalog", deviceId);
        
        using var message = new Message(Encoding.UTF8.GetBytes(messageBody));
        message.Ack = DeliveryAcknowledgement.Full;
        message.Properties["command-name"] = "increase-cooling";
        var hubClient = _iotHubs[registrationState.AssignedHub];
        await hubClient.SendAsync(deviceId, message);
        _commandsSentMetrics.IncrementCommandSent();
    }
}