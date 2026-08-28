using Auth.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Auth.IntegrationTests;

public class DbFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(ConnectionString);
        await using var context = new AuthDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public AuthDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AuthDbContext(options);
    }

    public async Task ClearTablesAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(ConnectionString);
        await using var context = new AuthDbContext(optionsBuilder.Options);

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"bookings\", \"events\", \"users\" RESTART IDENTITY CASCADE;");
    }
}