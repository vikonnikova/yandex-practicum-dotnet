using Events.Infrastructure.DataAccess;

namespace Events.Infrastructure.HealthChecker;

public class DatabaseHealthChecker(AppDbContext context) : IDatabaseHealthChecker
{
	public bool Check()
	{
		return context.Database.CanConnect();
	}
}