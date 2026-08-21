using Events.Application.QueryHandlers.Bookings;
using Events.Application.QueryHandlers.Events;
using Events.Application.UseCases.Events;
using Events.Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Application;

public static class DependencyInjection
{
	public static void AddApplicationServices(this IServiceCollection services)
	{
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetEventsByQueryHandler).Assembly));

		services.AddSingleton<GetEventsByQueryHandler>();
		services.AddSingleton<GetEventByIdQueryHandler>();
		services.AddSingleton<GetBookingByIdQueryHandler>();

		services.AddScoped<AddEventCommandHandler>();
		services.AddScoped<UpdateEventCommandHandler>();
		services.AddScoped<RemoveEventCommandHandler>();
		services.AddScoped<BookEventCommandHandler>();
		services.AddScoped<AddUserCommandHandler>();
	}
}