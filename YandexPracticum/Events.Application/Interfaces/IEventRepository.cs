using Events.Domain;

namespace Events.Application.Interfaces;

public interface IEventRepository
{
    Task<FilteredResult<Event>> GetFiltered(int page, int pageSize, Filters? filters,
        CancellationToken cancellationToken);

    Task<Event?> Find(Guid eventId, CancellationToken cancellationToken);

    void Add(Event @event);

    void Delete(Event @event);

    Task<bool> Exists(Guid eventId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}