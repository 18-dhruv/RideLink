using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using RideService.model;
public class RidePublish
{
    private readonly IProducer<int,string> _producer;
    private readonly ILogger<RidePublish> _logger;
    private readonly string Topic;
    public RidePublish(IOptions<KafkaSettings> options, ILogger<RidePublish> logger)
    {
        this._logger = logger;
        KafkaSettings settings = options.Value;

        var config=new ProducerConfig();
        config.BootstrapServers=settings.BootstrapServers;
         this._producer=new ProducerBuilder<int,string>(config).Build();
         this.Topic=settings.RideTopic;
    }
    public async Task PublishRideAsync(RideRequestedEvent evt)
    {
        var message =new Message<int,string>();
        message.Key=evt.RideId;
        message.Value=JsonSerializer.Serialize(evt);
        var result = await _producer.ProduceAsync(Topic,message);
        _logger.LogInformation("RideRequested sent: ride {RideId} partition {Partition} offset {Offset}",
            evt.RideId, result.Partition.Value, result.Offset.Value);
    }
     
}