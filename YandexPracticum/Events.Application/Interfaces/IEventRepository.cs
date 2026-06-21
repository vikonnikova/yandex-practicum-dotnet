using Events.Domain;

namespace Events.Application.Interfaces;

public interface IEventRepository
{
	Task<IReadOnlyCollection<Event>> GetAll(CancellationToken cancellationToken);

	Task<Event?> Find(Guid eventId, CancellationToken cancellationToken);

	Task Add(Event @event, CancellationToken cancellationToken);

	Task Update(CancellationToken cancellationToken);

	Task Delete(Event @event, CancellationToken cancellationToken);

	Task<bool> Exists(Guid eventId, CancellationToken cancellationToken);
}