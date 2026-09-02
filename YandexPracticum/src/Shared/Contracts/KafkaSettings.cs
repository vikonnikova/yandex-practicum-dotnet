namespace Shared.Contracts;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;

    public string ConsumerGroup { get; set; } = string.Empty;
}
