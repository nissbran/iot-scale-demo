using Confluent.Kafka;

namespace MessageRouter.Services;

public class LocalhostKafkaConnectionProvider : IKafkaConnectionProvider
{
    public AdminClientConfig GetAdminConfig() => new AdminClientConfig()
    {
        BootstrapServers = "localhost:9092,localhost:9093,localhost:9094",
        //BootstrapServers = "localhost:9092",
    };
    
    public ConsumerConfig GetConsumer(string groupId) => new ConsumerConfig
    {
        GroupId = groupId,
        BootstrapServers = "localhost:9092,localhost:9093,localhost:9094",
        //BootstrapServers = "localhost:9092",
        //GroupInstanceId = instanceId,
        // Note: The AutoOffsetReset property determines the start offset in the event
        // there are not yet any committed offsets for the consumer group for the
        // topic/partitions of interest. By default, offsets are committed
        // automatically, so in this example, consumption will only start from the
        // earliest message in the topic 'my-topic' the first time you run the program.
            
        AutoOffsetReset = AutoOffsetReset.Earliest,
        //MessageMaxBytes = 1000000,
        //FetchMaxBytes = 1000000,
    };
    
    public ConsumerConfig GetNoAutoCommitConsumerConfig(string? groupId = null!)
    {
        var config = GetConsumer(groupId);
        config.EnableAutoCommit = false;
        return config;
    }

    public ProducerConfig GetProducerConfig() => new ProducerConfig
    {
        BootstrapServers = "localhost:9092,localhost:9093,localhost:9094",
        //BootstrapServers = "localhost:9092",
        
        //Acks = Acks.Leader
        //MessageMaxBytes = 5_000_000,
    };

    public ProducerConfig GetPinnedProducerConfig()
    {
        var config = GetProducerConfig();
        config.Partitioner = Partitioner.Consistent;
        return config;
    }
    
    public ProducerConfig GetEventHubProducerConfig() => new ProducerConfig
    {
        BootstrapServers = "----.servicebus.windows.net:9093;",
        //MessageMaxBytes = 5_000_000,
        RequestTimeoutMs = 60000,
        SecurityProtocol = SecurityProtocol.SaslSsl,
        SaslMechanism = SaslMechanism.Plain,
        SaslUsername = "$ConnectionString",
        SaslPassword = "Endpoint=sb://----.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=---key--"
    };
}