using Confluent.Kafka;

namespace MessageRouter.Services;

public class EventHubKafkaConnectionProvider : IKafkaConnectionProvider
{
    private readonly string? _bootstrapServers;
    private readonly string? _eventHubConnectionString;
    
    public EventHubKafkaConnectionProvider(IConfiguration configuration)
    {
        _bootstrapServers = configuration["Kafka:BootstrapServers"];
        _eventHubConnectionString = configuration["Kafka:EventHubConnectionString"];
    }

    public AdminClientConfig GetAdminConfig()
    {
        throw new NotImplementedException();
    }

    public ConsumerConfig GetConsumer(string? groupId = null) => new ConsumerConfig
    {
        GroupId = groupId ?? KafkaDefaultConf.DefaultGroupId,
        BootstrapServers = _bootstrapServers,
        SecurityProtocol = SecurityProtocol.SaslSsl,
        SocketTimeoutMs = 60000,                //this corresponds to the Consumer config `request.timeout.ms`
        SessionTimeoutMs = 30000,
        SaslMechanism = SaslMechanism.Plain,
        SaslUsername = "$ConnectionString",
        SaslPassword = _eventHubConnectionString,
        //SslCaLocation = cacertlocation,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        BrokerVersionFallback = "1.0.0",        //Event Hubs for Kafka Ecosystems supports Kafka v1.0+, a fallback to an older API will fail
    };
    
    public ConsumerConfig GetNoAutoCommitConsumerConfig(string? groupId = null!)
    {
        var config = GetConsumer(groupId);
        config.EnableAutoCommit = false;
        return config;
    }

    public ProducerConfig GetProducerConfig()
    {
        throw new NotImplementedException();
    }

    public ProducerConfig GetPinnedProducerConfig()
    {
        throw new NotImplementedException();
    }
}