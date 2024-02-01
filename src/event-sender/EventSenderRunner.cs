using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventSender;

public class EventSenderRunner : IHostedService
{
    private readonly ILogger<EventSenderRunner> _logger;
    private readonly int _numberOfSenders;
    private readonly Task[] _tasks;
    private readonly List<EventBatchSender> _senders = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly string? _eventHubConnectionString;


    public EventSenderRunner(ILogger<EventSenderRunner> logger, IConfiguration configuration)
    {
        _logger = logger;
        _eventHubConnectionString = configuration["EventHub:ConnectionString"];
        _numberOfSenders = configuration.GetValue<int>("NumberOfSenders");
        _tasks = new Task[_numberOfSenders];
        
    }

    public Task StartAsync(CancellationToken cancellationToken)
    { 
        _logger.LogInformation("Event Sender started at: {time:s}, with {NumberOfSenders} sender", DateTimeOffset.Now, _numberOfSenders);
        
        var producerClient = new EventHubProducerClient(_eventHubConnectionString);
        for (int i = 0; i < _numberOfSenders; i++)
        {
            var sender = new EventBatchSender(producerClient, i + 1);
            _senders.Add(sender);
            _tasks[i] = Task.Run(async () =>
            {
                await sender.StartAsync(_cancellationTokenSource.Token);
            }, cancellationToken);
        }
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Senders...");
        foreach (var batchSender in _senders)
        {
            await batchSender.StopAsync(cancellationToken);
        }
        await Task.Delay(1000, cancellationToken);
        _logger.LogInformation("Event Sender stopped at: {time:s}", DateTimeOffset.Now);
    }
}