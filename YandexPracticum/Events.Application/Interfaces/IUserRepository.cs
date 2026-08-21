using Events.Domain;

namespace Events.Application.Interfaces;

public interface IUserRepository
{
	Task<User?> Find(Guid userId, CancellationToken cancellationToken);

	void Add(User user);

	Task SaveChangesAsync(CancellationToken cancellationToken);
}