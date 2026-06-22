using Events.Application.Interfaces;
using Events.Domain;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure;

public class EventRepository(AppDbContext context) : IEventRepository
{
	public async Task<IReadOnlyCollection<Event>> GetAll(CancellationToken cancellationToken)
	{
		return await context.Events.ToListAsync(cancellationToken);
	}

	public async Task<Event?> Find(Guid eventId, CancellationToken cancellationToken)
	{
		return await context.Events.FindAsync([eventId], cancellationToken);
	}

	public async Task Add(Event @event, CancellationToken cancellationToken)
	{
		context.Events.Add(@event);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task Update(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task Delete(Event @event, CancellationToken cancellationToken)
	{
		context.Events.Remove(@event);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> Exists(Guid eventId, CancellationToken cancellationToken)
	{
		return await context.Events.AnyAsync(e => e.Id == eventId, cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}
}