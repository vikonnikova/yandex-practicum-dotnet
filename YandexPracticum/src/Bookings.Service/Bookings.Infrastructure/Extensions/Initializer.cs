using Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure.Extensions;

public static class Initializer
{
    public static void InitDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        db.Database.Migrate();
    }
}