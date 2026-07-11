using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Events.IntegrationTests.Infrastructure;

public class DbFixture : IAsyncLifetime
{
	private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

	public string ConnectionString => _postgres.GetConnectionString();

	public async Task InitializeAsync()
	{
		await _postgres.StartAsync();

		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString);
		await using var context = new AppDbContext(optionsBuilder.Options);
		await context.Database.MigrateAsync();
	}

	public async Task DisposeAsync()
	{
		await _postgres.DisposeAsync();
	}
}