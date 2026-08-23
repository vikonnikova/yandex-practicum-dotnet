using Events.Application.QueryHandlers.Bookings;
using Events.Application.QueryHandlers.Events;
using Events.Application.QueryHandlers.Users;
using Events.Application.UseCases.Events;
using Events.Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Application;

public static class DependencyInjection
{
	public static void AddApplicationServices(this IServiceCollection services)
	{
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetEventsByQueryHandler).Assembly));

		services.AddScoped<GetUserByIdQueryHandler>();
		services.AddScoped<GetEventsByQueryHandler>();
		services.AddScoped<GetEventByIdQueryHandler>();
		services.AddScoped<GetBookingByIdQueryHandler>();

		services.AddScoped<AddEventCommandHandler>();
		services.AddScoped<UpdateEventCommandHandler>();
		services.AddScoped<RemoveEventCommandHandler>();
		services.AddScoped<BookEventCommandHandler>();
		services.AddScoped<AddUserCommandHandler>();
	}
}