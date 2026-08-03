namespace Events.Infrastructure.HealthChecker;

public interface IDatabaseHealthChecker
{
	bool Check();
}