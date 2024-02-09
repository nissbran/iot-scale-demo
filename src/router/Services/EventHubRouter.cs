using MessageRouter.Telemetry;

namespace MessageRouter.Services;

public class ConsumerService : BackgroundService
{
    private readonly List<IotEventsConsumer> _consumers = new();
        
    public ConsumerService(IKafkaConnectionProvider connectionProvider, MessageMediator messageMediator, IConfiguration configuration, EventConsumedMetrics metrics)
    {
        var topic = configuration["Kafka:Topic"];
        
        for (int i = 0; i < 1; i++)
        {
            _consumers.Add(new IotEventsConsumer(connectionProvider.GetConsumer("consumer-group"), messageMediator, metrics, i, topic ?? "events"));
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumer in _consumers)
        {
            consumer.Start(stoppingToken);
        }
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        foreach (var consumer in _consumers)
        {
            consumer.Stop();
        }
        base.Dispose();
    }
}