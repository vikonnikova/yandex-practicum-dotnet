using Events.Application.Interfaces;
using Events.Infrastructure.BackgroundServices;
using Events.Infrastructure.DataAccess;
using Events.Infrastructure.HealthChecker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IDatabaseHealthChecker, DatabaseHealthChecker>();

        services.AddSingleton(BindKafkaSettings(configuration));
        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmedConsumer>();
    }

    private static KafkaSettings BindKafkaSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>()
                       ?? throw new InvalidOperationException($"Секция конфигурации '{KafkaSettings.SectionName}' не найдена.");

        if (string.IsNullOrWhiteSpace(settings.BootstrapServers))
        {
            throw new InvalidOperationException("Configuration value 'Kafka:BootstrapServers' not found.");
        }

        if (string.IsNullOrWhiteSpace(settings.ConsumerGroup))
        {
            throw new InvalidOperationException("Configuration value 'Kafka:ConsumerGroup' not found.");
        }

        return settings;
    }
}