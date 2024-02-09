using Confluent.Kafka;
using MessageRouter.Contract;
using MessageRouter.Telemetry;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.Amqp.Encoding;
using Serilog;

namespace MessageRouter.Services;

public class IotEventsConsumer
{
    private readonly MessageMediator _messageMediator;
    private readonly EventConsumedMetrics _metrics;
    private readonly int _number;
    private readonly string _topic;
    // How often you ack commit, 1 is every message
    private readonly int _commitPeriod;
    private readonly IConsumer<Ignore, string> _consumer;
    private Task? _consumerTask;
    private bool _stopping = false;
    
    public IotEventsConsumer(ConsumerConfig config, MessageMediator messageMediator, EventConsumedMetrics metrics, int number, string topic, int commitPeriod = 1)
    {
        _messageMediator = messageMediator;
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
    
    private async Task Run(CancellationToken cancellationToken)
    {
        Log.Information("Starting consumer on topic {Topic}", _topic);
        _consumer.Subscribe(_topic);

        while (!(cancellationToken.IsCancellationRequested || _stopping))
        {
            try
            {
                var messageConsumeResult = _consumer.Consume(cancellationToken);
                
                var message = ParseKafkaMessage(messageConsumeResult.Message);
                
                _metrics.IncrementMessages();
                
                //Log.Information("Message consumed Type: {Type}, Client: {Client}, Partition: {Partition}, Offset: {Offset}, Hub: {Hub}", 
                //    message.Type,
                //    message.DeviceId,
                //    messageConsumeResult.Partition.Value, 
                //    messageConsumeResult.Offset,
                //    "");
                
                var typedMessage = MessageDeserializer.Deserialize(message);
                
                if (typedMessage == null)
                {
                    Log.Warning("Message type not found {Type}, source: {Source}", message.Type, message.Source);
                    continue;
                }
                
                await _messageMediator.RouteMessage(typedMessage);
                
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

    }

    private IotMessage ParseKafkaMessage(Message<Ignore, string> message)
    {
        var deviceId = "";
        var messageSource = "";
        var hub = "";
        var type = "";
        var opType = "";
                
        foreach (var header in message.Headers)
        {
            switch (header.Key)
            {
                case "iothub-connection-device-id":
                    deviceId = ParseHeader(header);
                    break;
                case "iothub-message-source":
                    messageSource = ParseHeader(header);
                    break;
                case "opType":
                    opType = ParseHeader(header);
                    break;
                case "hubName":
                    hub = ParseHeader(header);
                    break;
                case "type":
                    type = ParseHeader(header);
                    break;
            }
        }
        
        return new IotMessage(deviceId, type, messageSource, message.Value, hub, opType);
    }

    private static string? ParseHeader(IHeader header)
    {
        var headerValueBytes = header.GetValueBytes();
        var value = AmqpEncoding.DecodeObject(new ByteBuffer(headerValueBytes, 0, headerValueBytes.Length));
        var messageSource = value.ToString();
        return messageSource;
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