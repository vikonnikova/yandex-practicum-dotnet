using Events.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure;

public static class DependencyInjection
{
	public static void AddInfrastructureServices(this IServiceCollection services)
	{
		services.AddSingleton<IEventRepository, InMemoryEventStore>();
		services.AddSingleton<IBookingRepository, InMemoryBookingStore>();
		
		//services.AddHostedService<BookingService>();
	}
}