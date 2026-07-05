using Events.Application;
using Events.Application.Interfaces;
using Events.Domain;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure;

internal class EventRepository(AppDbContext context) : IEventRepository
{
	public async Task<FilteredResult<Event>> GetFiltered(int page, int pageSize, Filters filters,
		CancellationToken cancellationToken)
	{
		var query = context.Events.AsQueryable();

		if (filters.Title != null)
		{
			query = query.Where(x => x.Title.Contains(filters.Title));
		}

		if (filters.From.HasValue)
		{
			query = query.Where(x => x.Period.StartAt >= filters.From);
		}

		if (filters.To.HasValue)
		{
			query = query.Where(x => x.Period.EndAt <= filters.To);
		}

		var totalItems = await query.CountAsync(cancellationToken);
		var result = query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x).ToArray();

		return new FilteredResult<Event>(totalItems, result);
	}

	public async Task<Event?> Find(Guid eventId, CancellationToken cancellationToken)
	{
		return await context.Events.FindAsync([eventId], cancellationToken);
	}

	public void Add(Event @event, CancellationToken cancellationToken)
	{
		context.Events.Add(@event);
	}

	public void Delete(Event @event, CancellationToken cancellationToken)
	{
		context.Events.Remove(@event);
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