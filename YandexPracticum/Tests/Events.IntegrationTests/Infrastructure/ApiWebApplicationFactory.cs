using Events.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Events.IntegrationTests.Infrastructure;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly DbFixture _dbFixture = new();
	
	public ApiWebApplicationFactory()
	{
		_dbFixture.InitializeAsync().GetAwaiter().GetResult();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			var descriptor = services.SingleOrDefault(d =>
				d.ServiceType == typeof(DbContextOptions<AppDbContext>));

			if (descriptor != null)
			{
				services.Remove(descriptor);
			}

			services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(_dbFixture.ConnectionString); });
		});
	}
	
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_dbFixture.DisposeAsync().GetAwaiter().GetResult();
		}
		base.Dispose(disposing);
	}
}