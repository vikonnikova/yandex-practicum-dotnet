using Events.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Application;

public static class DependencyInjection
{
	public static void AddApplicationServices(this IServiceCollection services)
	{
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
	}
}