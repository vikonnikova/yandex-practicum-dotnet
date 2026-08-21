using Events.Application.Interfaces;
using Events.Domain;
using Events.Infrastructure.DataAccess;

namespace Events.Infrastructure;

internal class UserRepository(AppDbContext context) : IUserRepository
{
	public async Task<User?> Find(Guid userId, CancellationToken cancellationToken)
	{
		return await context.Users.FindAsync([userId], cancellationToken: cancellationToken);
	}

	public void Add(User user)
	{
		context.Users.Add(user);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}
}