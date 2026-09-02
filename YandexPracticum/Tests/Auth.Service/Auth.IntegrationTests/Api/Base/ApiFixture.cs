using Auth.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Auth.IntegrationTests.Api.Base;

public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private ApiWebApplicationFactory Factory { get; set; } = null!;
    private string ConnectionString => _postgres.GetConnectionString();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Factory = new ApiWebApplicationFactory(ConnectionString);
        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }

        if (_postgres != null)
        {
            await _postgres.DisposeAsync();
        }
    }

    public async Task ClearTablesAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(ConnectionString);
        await using var context = new AuthDbContext(optionsBuilder.Options);

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"users\" RESTART IDENTITY CASCADE;");
    }

    public async Task ExecuteDbContextAsync(Func<AuthDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await action(dbContext);

        await dbContext.SaveChangesAsync();
    }
}