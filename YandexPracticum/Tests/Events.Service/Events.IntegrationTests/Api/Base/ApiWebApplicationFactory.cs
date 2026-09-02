using Events.Infrastructure.BackgroundServices;
using Events.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Events.IntegrationTests.Api.Base;

public class ApiWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    public FakeTimeProvider FakeTime { get; } = new(new DateTimeOffset(2025, 12, 31, 12, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(AppContext.BaseDirectory);
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json"),
                optional: false);
        });

        return base.CreateHost(builder);
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        AddCommonTestServices(services);

        RemoveHostedService<BookingConfirmedConsumer>(services);
        RemoveHostedService<KafkaTopicInitializer>(services);
    }

    protected void AddCommonTestServices(IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(FakeTime);

        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<AppDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(connectionString); });

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, options => { });
    }

    private static void RemoveHostedService<THostedService>(IServiceCollection services)
        where THostedService : class, IHostedService
    {
        var descriptors = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && (descriptor.ImplementationType == typeof(THostedService)
                    || descriptor.ImplementationInstance is THostedService))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
