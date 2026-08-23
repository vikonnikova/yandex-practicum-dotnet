using Events.Application.Interfaces;
using Events.Domain;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure;

internal class UserRepository(AppDbContext context) : IUserRepository
{
	public async Task<User?> Find(Guid userId, CancellationToken cancellationToken)
	{
		return await context.Users.FindAsync([userId], cancellationToken: cancellationToken);
	}
	
	public async Task<User?> FindByLogin(string login, CancellationToken cancellationToken)
	{
		return await context.Users.FirstOrDefaultAsync(x => x.Login == login, cancellationToken: cancellationToken);
	}
	
	public async Task<bool> ExistsByLogin(string login, CancellationToken cancellationToken)
	{
		return await context.Users.AnyAsync(x => x.Login == login, cancellationToken: cancellationToken);
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