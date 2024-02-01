using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Serilog;

namespace EventSender;

public class EventBatchSender
{
    private readonly EventHubProducerClient _client;
    private readonly int _senderNumber;
    private const int BatchSize = 100;
    private bool _stopping = false;
    private long _numberOfMessages = 0;

    public EventBatchSender(EventHubProducerClient client, int senderNumber)
    {
        _client = client;
        _senderNumber = senderNumber;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var deviceId = $"client_sim_{_senderNumber:000}";

        while (!(cancellationToken.IsCancellationRequested || _stopping))
        {
            Log.Information("Sending batch of {BatchSize} events from {DeviceId}", BatchSize, deviceId);
            var batch = await _client.CreateBatchAsync(cancellationToken);
            for (var i = 0; i < BatchSize; i++)
            {
                var eventData = new EventData(Encoding.UTF8.GetBytes($"{{\"deviceId\": \"{deviceId}\", \"temperature\": {Random.Shared.Next(1, 60)}}}"));
                eventData.Properties.Add("iothub-connection-device-id", deviceId);
                eventData.Properties.Add("iothub-message-source", "Telemetry");
                eventData.Properties.Add("hub", "event-sender");
                batch.TryAdd(eventData);
                _numberOfMessages++;
            }
            await _client.SendAsync(batch, cancellationToken);
        }
        
        Log.Information("Sender {SenderNumber} stopped sending messages after {NumberOfMessages} messages.", _senderNumber, _numberOfMessages);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stopping = true;
        return Task.CompletedTask;
    }
}