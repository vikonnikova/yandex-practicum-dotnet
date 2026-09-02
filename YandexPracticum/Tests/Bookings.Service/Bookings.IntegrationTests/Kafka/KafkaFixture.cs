using Testcontainers.Kafka;

namespace Bookings.IntegrationTests.Kafka;

public class KafkaFixture : IAsyncLifetime
{
    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.6.1").Build();

    public string BootstrapServers { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _kafka.StartAsync();
        BootstrapServers = ToBootstrapServers(_kafka.GetBootstrapAddress());
    }

    public async Task DisposeAsync()
    {
        await _kafka.DisposeAsync();
    }

    private static string ToBootstrapServers(string address)
    {
        return Uri.TryCreate(address, UriKind.Absolute, out var uri)
            ? $"{uri.Host}:{uri.Port}"
            : address;
    }
}
