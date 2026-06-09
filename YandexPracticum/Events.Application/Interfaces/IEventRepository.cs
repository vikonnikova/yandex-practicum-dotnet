using Events.Domain;

namespace Events.Application.Interfaces;

public interface IEventRepository
{
	IReadOnlyCollection<Event> GetAll();
	
	Event? Find(Guid eventId);

	void Add(Event @event);

	void Delete(Event @event);
}