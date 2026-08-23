using Events.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Events.IntegrationTests.Api.Base;

public class ApiWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Development");

		builder.ConfigureTestServices(services =>
		{
			var descriptor = services.SingleOrDefault(d =>
				d.ServiceType == typeof(DbContextOptions<AppDbContext>));
			if (descriptor != null) services.Remove(descriptor);

			services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(connectionString); });
		});
	}
}