using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Events.IntegrationTests.Api.Base;

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
		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString);
		await using var context = new AppDbContext(optionsBuilder.Options);

		await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"bookings\", \"events\" RESTART IDENTITY CASCADE;");
	}
}