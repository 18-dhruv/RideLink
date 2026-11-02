namespace Driver.config;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "";
    public string RideTopic { get; set; } = "";
    public string GroupId { get; set; } = "";
}
