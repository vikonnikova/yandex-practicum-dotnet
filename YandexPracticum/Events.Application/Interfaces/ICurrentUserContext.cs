namespace Events.Application.Interfaces;

public interface ICurrentUserContext
{
	Guid UserId { get; }
	bool IsAuthenticated { get; }
}