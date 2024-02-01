using System.Text.Json;
using Confluent.Kafka;
using Serilog;

namespace MessageRouter.Services;

public class SimpleConsumer
{
    private readonly int _number;
    private readonly string _topic;
    private readonly IConsumer<Ignore, string> _consumer;
    private Task? _consumerTask;
    private bool _stopping = false;
    private long _numberOfMessages;
    private int _delayMs;

    public SimpleConsumer(ConsumerConfig config, int number, string topic = KafkaDefaultConf.DefaultTopic, int delayMs = 10)
    {
        _number = number;
        _topic = topic;
        _delayMs = delayMs;
        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    public void Start(CancellationToken cancellationToken)
    {
        _consumerTask = Task.Run(async () => await Run(cancellationToken), cancellationToken);
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);

        while (!(cancellationToken.IsCancellationRequested || _stopping))
        {
            try
            {
                var messageConsumeResult = _consumer.Consume(cancellationToken);

                _numberOfMessages++;
                var message = JsonSerializer.Deserialize(messageConsumeResult.Message.Value, typeof(DefaultMessage));
                var defaultMessage = message as DefaultMessage;
                Log.Information("Consumer {Number}, Partition: {Partition}, Offset: {Offset}, PartitionOffset: {PartitionOffset}, CurrentCount: {Count}, {Sequence}",
                    _number, messageConsumeResult.Partition.Value, messageConsumeResult.Offset.Value, messageConsumeResult.TopicPartitionOffset.Offset.Value,
                    _numberOfMessages, defaultMessage?.Sequence);
                await Task.Delay(_delayMs, cancellationToken);
                //  Consumer 1, Partition: 2, Offset: 2637, PartitionOffset: 2637, CurrentCount: 1, 060008
                //throw new NotImplementedException();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                Log.Error(e, "Consume error: {Reason}",e.Error.Reason);

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Unexpected error: {Reason}",e.Message);
                break;
            }
        }
            
        Log.Information("Number of messages fetched: {NumberOfMessages}", _numberOfMessages);
        _consumer.Close();
    }

    public void Stop()  
    {
        Log.Information("Stopping consumer");
        _stopping = true;
        _consumerTask?.Wait(5000);
        _consumer.Close();
        _consumer.Dispose();
        Log.Information("Consumer stopped");
    }
}