using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;

namespace Events.IntegrationTests.Kafka;

public class KafkaFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.6.1").Build();
    private KafkaApiWebApplicationFactory Factory { get; set; } = null!;

    public string BootstrapServers { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _kafka.StartAsync());

        BootstrapServers = ToBootstrapServers(_kafka.GetBootstrapAddress());

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_postgres.GetConnectionString());
        await using (var context = new AppDbContext(optionsBuilder.Options))
        {
            await context.Database.MigrateAsync();
        }

        Factory = new KafkaApiWebApplicationFactory(_postgres.GetConnectionString(), BootstrapServers);
        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }

        await _kafka.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public async Task ClearTablesAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_postgres.GetConnectionString());
        await using var context = new AppDbContext(optionsBuilder.Options);
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"events\" RESTART IDENTITY CASCADE;");
    }

    public async Task ExecuteDbContextAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await action(dbContext);
        await dbContext.SaveChangesAsync();
    }

    private static string ToBootstrapServers(string address)
    {
        return Uri.TryCreate(address, UriKind.Absolute, out var uri)
            ? $"{uri.Host}:{uri.Port}"
            : address;
    }
}
