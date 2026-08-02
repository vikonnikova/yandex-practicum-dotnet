using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Events.IntegrationTests.Api.Base;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");
	}
}