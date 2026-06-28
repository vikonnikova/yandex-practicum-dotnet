using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Events.IntegrationTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	public string ConnectionString { get; } =
		"Host=localhost;Port=5432;Database=events_test;Username=postgres;Password=postgres";

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");
		builder.ConfigureAppConfiguration((context, config) =>
		{
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "ConnectionStrings:Default", ConnectionString }
			});
		});
	}
}

// TODO подменять реальную БД тестовой