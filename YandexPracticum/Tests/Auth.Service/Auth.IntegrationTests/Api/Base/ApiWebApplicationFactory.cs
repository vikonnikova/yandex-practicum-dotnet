using Auth.Infrastructure.DataAccess;
using Events.IntegrationTests.Api.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Auth.IntegrationTests.Api.Base;

public class ApiWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    public FakeTimeProvider FakeTime { get; } = new(new DateTimeOffset(2025, 12, 31, 12, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // TimeProvider
            services.AddSingleton<TimeProvider>(FakeTime);

            //DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<AuthDbContext>(options => { options.UseNpgsql(connectionString); });

            //Auth
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });
        });
    }
}