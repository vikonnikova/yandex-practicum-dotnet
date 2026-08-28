using Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Bookings.IntegrationTests;

public class DbFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var optionsBuilder = new DbContextOptionsBuilder<BookingsDbContext>().UseNpgsql(ConnectionString);
        await using var context = new BookingsDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public BookingsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new BookingsDbContext(options);
    }

    public async Task ClearTablesAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BookingsDbContext>().UseNpgsql(ConnectionString);
        await using var context = new BookingsDbContext(optionsBuilder.Options);

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"bookings\" RESTART IDENTITY CASCADE;");
    }
}