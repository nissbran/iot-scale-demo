using Confluent.Kafka;

namespace MessageMediator.Services.Kafka;

public interface IKafkaConnectionProvider
{

    AdminClientConfig GetAdminConfig();
    ConsumerConfig GetConsumer(string groupId);
    ConsumerConfig GetNoAutoCommitConsumerConfig(string? groupId = null!);
    ProducerConfig GetProducerConfig();
    ProducerConfig GetPinnedProducerConfig();
}