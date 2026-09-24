using Events.Application;
using Events.Application.Interfaces;
using Events.Infrastructure.BackgroundServices;
using Events.Infrastructure.Caching;
using Events.Infrastructure.DataAccess;
using Events.Infrastructure.HealthChecks;
using Events.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Contracts;
using Shared.Settings;
using StackExchange.Redis;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Не найдена строка подключения к БД.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        services.AddSingleton(BindKafkaSettings(configuration));
        services.AddHostedService<KafkaTopicInitializer>();

        services.AddSingleton(BindCacheSettings(configuration));
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            CreateRedisConnection(sp.GetRequiredService<CacheSettings>()));
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddHostedService<BookingConfirmedConsumer>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("postgres", tags: ["ready"])
            .AddCheck<RedisHealthCheck>("redis", tags: ["ready"])
            .AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);
    }

    private static KafkaSettings BindKafkaSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>()
                       ?? throw new InvalidOperationException(
                           $"Секция конфигурации '{KafkaSettings.SectionName}' не найдена.");

        if (string.IsNullOrWhiteSpace(settings.BootstrapServers))
        {
            throw new InvalidOperationException(
                "Не найден адрес брокера Kafka в конфигурации [секция 'Kafka:BootstrapServers'].");
        }

        if (string.IsNullOrWhiteSpace(settings.ConsumerGroup))
        {
            throw new InvalidOperationException(
                "Не найден идентификатор группы потребителей Kafka в конфигурации [секция 'Kafka:ConsumerGroup'].");
        }

        return settings;
    }

    private static CacheSettings BindCacheSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>()
                       ?? new CacheSettings();

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            settings.ConnectionString = "localhost:6379";
        }

        if (settings.EventTtlSeconds <= 0)
        {
            settings.EventTtlSeconds = 300;
        }

        if (settings.TopEventsTtlSeconds <= 0)
        {
            settings.TopEventsTtlSeconds = 60;
        }

        return settings;
    }

    private static IConnectionMultiplexer CreateRedisConnection(CacheSettings settings)
    {
        var options = ConfigurationOptions.Parse(settings.ConnectionString);
        options.AbortOnConnectFail = false;
        options.ConnectRetry = 1;
        options.ConnectTimeout = 1000;
        return ConnectionMultiplexer.Connect(options);
    }
}