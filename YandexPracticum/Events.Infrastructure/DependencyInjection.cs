using Events.Application.Interfaces;
using Events.Infrastructure.BackgroundServices;
using Events.Infrastructure.DataAccess;
using Events.Infrastructure.HealthChecker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure;

public static class DependencyInjection
{
	public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("Default")
		                       ?? throw new InvalidOperationException("Connection string 'Default' not found.");

		services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

		services.AddScoped<IEventRepository, EventRepository>();
		services.AddScoped<IBookingRepository, BookingRepository>();
		services.AddScoped<IDatabaseHealthChecker, DatabaseHealthChecker>();

		services.AddHostedService<BookingBackgroundService>();
	}
}