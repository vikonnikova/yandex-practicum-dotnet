using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Events.Infrastructure.BackgroundServices;

internal sealed class KafkaTopicInitializer(
    KafkaSettings settings,
    ILogger<KafkaTopicInitializer> logger) : IHostedService
{
    private static readonly TimeSpan MetadataTimeout = TimeSpan.FromSeconds(10);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var topicName = KafkaConstants.BookingConfirmedTopic;

        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = settings.BootstrapServers
            }).Build();

            if (TopicExists(adminClient, topicName))
            {
                logger.LogInformation("Kafka topic '{Topic}' already exists.", topicName);
                return;
            }

            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            ]);

            logger.LogInformation("Kafka topic '{Topic}' created.", topicName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Kafka topic '{Topic}'. Application startup will continue.", topicName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static bool TopicExists(IAdminClient adminClient, string topicName)
    {
        var metadata = adminClient.GetMetadata(MetadataTimeout);
        return metadata.Topics.Any(topic =>
            topic.Topic == topicName && !topic.Error.IsError);
    }
}
