using Bookings.Application.Interfaces;
using Bookings.Infrastructure.BackgroundServices;
using Bookings.Infrastructure.DataAccess;
using Bookings.Infrastructure.Repositories;
using Bookings.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<BookingsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        services.AddSingleton(BindKafkaSettings(configuration));
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        services.AddHostedService<BookingBackgroundService>();
    }

    private static KafkaSettings BindKafkaSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>()
                       ?? throw new InvalidOperationException($"Секция конфигурации '{KafkaSettings.SectionName}' не найдена.");

        if (string.IsNullOrWhiteSpace(settings.BootstrapServers))
        {
            throw new InvalidOperationException("Configuration value 'Kafka:BootstrapServers' not found.");
        }

        return settings;
    }
}