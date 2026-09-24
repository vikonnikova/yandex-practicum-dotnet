using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Settings;

namespace Bookings.Infrastructure.HealthChecks;

internal sealed class KafkaHealthCheck(KafkaSettings settings) : IHealthCheck
{
    private static readonly TimeSpan MetadataTimeout = TimeSpan.FromSeconds(3);

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = settings.BootstrapServers
            }).Build();

            var metadata = admin.GetMetadata(MetadataTimeout);
            return Task.FromResult(metadata.Brokers.Count > 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Kafka returned no brokers."));
        }
        catch (Exception exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka is unavailable.", exception));
        }
    }
}
