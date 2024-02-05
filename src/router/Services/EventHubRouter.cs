using MessageRouter.Telemetry;

namespace MessageRouter.Services;

public class ConsumerService : BackgroundService
{
    //private readonly RobustConsumer _robustConsumer;
    
    private readonly List<RobustConsumer> _robustConsumers = new List<RobustConsumer>();
        
    public ConsumerService(IKafkaConnectionProvider connectionProvider, MessageMediator messageMediator, IConfiguration configuration, EventConsumedMetrics metrics)
    {
        var topic = configuration["Kafka:Topic"];
        
        for (int i = 0; i < 10; i++)
        {
            _robustConsumers.Add(new RobustConsumer(connectionProvider.GetConsumer("consumer-group"), messageMediator, metrics, i, topic ?? "events"));
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumer in _robustConsumers)
        {
            consumer.Start(stoppingToken);
        }
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        foreach (var consumer in _robustConsumers)
        {
            consumer.Stop();
        }
        base.Dispose();
    }
}