using Events.Application;
using Events.Application.Interfaces;
using Events.Domain;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace Events.Infrastructure;

internal class EventRepository(AppDbContext context) : IEventRepository
{
    public async Task<PaginatedResult<Event>> GetFiltered(int page, int pageSize, Filters? filters,
        CancellationToken cancellationToken)
    {
        var query = context.Events.AsQueryable();

        if (filters is not null)
        {
            if (filters.Title != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{filters.Title}%"));
            }

            if (filters.From.HasValue)
            {
                query = query.Where(x => x.Period.StartAt >= filters.From);
            }

            if (filters.To.HasValue)
            {
                query = query.Where(x => x.Period.EndAt <= filters.To);
            }
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var result = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x)
            .ToArrayAsync(cancellationToken);

        return new PaginatedResult<Event>(result, totalItems);
    }

    public async Task<Event?> Find(Guid eventId, CancellationToken cancellationToken)
    {
        return await context.Events.FindAsync([eventId], cancellationToken);
    }

    public void Add(Event @event)
    {
        context.Events.Add(@event);
    }

    public void Delete(Event @event)
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