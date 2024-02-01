using MessageRouter.Telemetry;

namespace MessageRouter.Services;

public class ConsumerService : BackgroundService
{
    //private readonly RobustConsumer _robustConsumer;
    
    private readonly List<RobustConsumer> _robustConsumers = new List<RobustConsumer>();
        
    public ConsumerService(IKafkaConnectionProvider connectionProvider, IConfiguration configuration, EventConsumedMetrics metrics)
    {
        var topic = configuration["Kafka:Topic"];
        //_robustConsumer = new RobustConsumer(connectionProvider.GetNoAutoCommitConsumerConfig("consumer-group"), metrics, 1, "iot-events");

        for (int i = 0; i < 10; i++)
        {
            _robustConsumers.Add(new RobustConsumer(connectionProvider.GetConsumer("consumer-group"), metrics, i, topic ?? "events"));
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