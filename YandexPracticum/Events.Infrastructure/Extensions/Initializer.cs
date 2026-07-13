using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure;

public static class Initializer
{
	public static void InitDatabase(this IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		db.Database.Migrate();
	}
}