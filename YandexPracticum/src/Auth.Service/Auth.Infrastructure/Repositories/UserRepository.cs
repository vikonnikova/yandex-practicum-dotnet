using Auth.Application.Interfaces;
using Auth.Domain;
using Auth.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

internal class UserRepository(AuthDbContext context) : IUserRepository
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