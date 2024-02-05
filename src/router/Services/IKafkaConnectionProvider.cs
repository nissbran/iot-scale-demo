using Confluent.Kafka;

namespace MessageRouter.Services;

public interface IKafkaConnectionProvider
{

    AdminClientConfig GetAdminConfig();
    ConsumerConfig GetConsumer(string groupId);
    ConsumerConfig GetNoAutoCommitConsumerConfig(string? groupId = null!);
    ProducerConfig GetProducerConfig();
    ProducerConfig GetPinnedProducerConfig();
}