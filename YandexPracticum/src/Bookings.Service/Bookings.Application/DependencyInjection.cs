using Bookings.Application.QueryHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetBookingByIdQueryHandler).Assembly));
    }
}