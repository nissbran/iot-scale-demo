using Confluent.Kafka;
using MessageRouter.Telemetry;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.Amqp.Encoding;
using Serilog;

namespace MessageRouter.Services;

public class RobustConsumer
{
    private readonly EventConsumedMetrics _metrics;
    private readonly int _number;
    private readonly string _topic;
    // How often you ack commit, 1 is every message
    private readonly int _commitPeriod;
    private readonly IConsumer<Ignore, string> _consumer;
    private Task? _consumerTask;
    private bool _stopping = false;
    
    public RobustConsumer(ConsumerConfig config, EventConsumedMetrics metrics, int number, string topic = KafkaDefaultConf.DefaultTopic, int commitPeriod = 1)
    {
        _metrics = metrics;
        _number = number;
        _topic = topic;
        _commitPeriod = commitPeriod;
        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }
    
    public void Start(CancellationToken cancellationToken)
    {
        _consumerTask = Task.Run(async () => await Run(cancellationToken), cancellationToken);
    }
    
    private Task Run(CancellationToken cancellationToken)
    {
        Log.Information("Starting consumer on topic {Topic}", _topic);
        _consumer.Subscribe(_topic);

        while (!(cancellationToken.IsCancellationRequested || _stopping))
        {
            try
            {
                var messageConsumeResult = _consumer.Consume(cancellationToken);

                var client = "";
                var messageSource = "";
                var hub = "";
                
                foreach (var header in messageConsumeResult.Message.Headers)
                {
                    switch (header.Key)
                    {
                        case "iothub-connection-device-id":
                        {
                            var headerValueBytes = header.GetValueBytes();
                            var value = AmqpEncoding.DecodeObject(new ByteBuffer(headerValueBytes, 0, headerValueBytes.Length));
                            client = value.ToString();
                            break;
                        }
                        case "iothub-message-source":
                        {
                            var headerValueBytes = header.GetValueBytes();
                            var value = AmqpEncoding.DecodeObject(new ByteBuffer(headerValueBytes, 0, headerValueBytes.Length));
                            messageSource = value.ToString();
                            break;
                        }
                        case "hub":
                        {
                            var headerValueBytes = header.GetValueBytes();
                            var value = AmqpEncoding.DecodeObject(new ByteBuffer(headerValueBytes, 0, headerValueBytes.Length));
                            hub = value.ToString();
                            break;
                        }
                    }
                }
                
                _metrics.IncrementMessages();
                // var message = JsonSerializer.Deserialize(messageConsumeResult.Message.Value, typeof(DefaultMessage));
                // var defaultMessage = message as DefaultMessage;
                Log.Information("Message consumed Type: {Type}, Client: {Client}, Partition: {Partition}, Offset: {Offset}, Hub: {Hub}", 
                    messageSource,
                    client,
                    messageConsumeResult.Partition.Value, 
                    messageConsumeResult.Offset,
                    hub);
                
                
                // if (messageConsumeResult.Offset % _commitPeriod == 0)
                //     _consumer.Commit(messageConsumeResult);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                Log.Error(e, "Consume error {Reason}", e.Error.Reason);

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e,"Unexpected error");
                break;
            }
        }
            
        //Log.Information("Number of messages fetched: {NumberOfMessages}", _metrics.);

        return Task.CompletedTask;
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