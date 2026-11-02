using Ride.model;
using Microsoft.Extensions.Options;
using Driver.config;
using Confluent.Kafka;
using System.Text.Json;
namespace Driver.service;

public class ConsumeRideEvent:BackgroundService
{
    private KafkaSettings _settings { get; set; }
    private ILogger<ConsumeRideEvent> _logger { get; set; }
    public ConsumeRideEvent(IOptions<KafkaSettings> options, ILogger<ConsumeRideEvent> logger)
    {
        this._settings = options.Value;
        this._logger = logger;
    }
    protected override  Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(()=>RunLoop(stoppingToken),stoppingToken);
    }
  
    private void RunLoop(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig();
        config.BootstrapServers = _settings.BootstrapServers;
        config.GroupId = _settings.GroupId;
        config.AutoOffsetReset = AutoOffsetReset.Earliest;
        config.EnableAutoCommit = false;
        using var consumer = new ConsumerBuilder<int, string>(config).Build();
        consumer.Subscribe(_settings.RideTopic);
        _logger.LogInformation("listenting on {Topic} as {Group}", _settings.RideTopic, _settings.GroupId);


        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = consumer.Consume(cancellationToken);  //wait until a event is produced here result represent a offset

                var ride = JsonSerializer.Deserialize<RideRequestedEvent>(result.Message.Value);

                if (ride == null)
                {
                    _logger.LogInformation("skipping the message at {OffSet}", result.Offset.Value);
                }
                else
                {
                    _logger.LogInformation("ride {RideID} requested at {Lat} {Long} (partiton{P} at offset {O})",ride.RideId, ride.PickupCoordinate.Latitude, ride.PickupCoordinate.Longitude, result.Partition.Value, result.Offset.Value);

                    //feed this ride to redis geo.

                }
                consumer.Commit(result);
            }
        }
        catch (OperationCanceledException){}
        finally
        {
            consumer.Close();
        }
    }
}