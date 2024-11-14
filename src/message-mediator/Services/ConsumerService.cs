using MediatR;
using MessageMediator.Services.Kafka;
using MessageMediator.Telemetry;

namespace MessageMediator.Services;

public class ConsumerService : BackgroundService
{
    private readonly List<IotEventsConsumer> _consumers = new();
        
    public ConsumerService(IKafkaConnectionProvider connectionProvider, IMediator messageMediator, IConfiguration configuration, EventConsumedMetrics metrics)
    {
        var topic = configuration["Kafka:Topic"];
        var numberOfConsumers = int.Parse(configuration["Kafka:NumberOfConsumers"] ?? "1");
        
        for (int i = 0; i < numberOfConsumers; i++)
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