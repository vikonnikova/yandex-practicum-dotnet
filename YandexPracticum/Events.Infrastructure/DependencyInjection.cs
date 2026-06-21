using Events.Application.Interfaces;
using Events.Infrastructure.BackgroundServices;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure;

public static class DependencyInjection
{
	public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(options =>
			options.UseNpgsql(configuration.GetConnectionString("Default")));

		services.AddSingleton<IEventRepository, EventRepository>();
		services.AddSingleton<IBookingRepository, BookingRepository>();
		services.AddHostedService<BookingBackgroundService>();
	}
}