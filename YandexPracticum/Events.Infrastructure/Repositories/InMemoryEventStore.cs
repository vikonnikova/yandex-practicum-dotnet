using Events.Domain;

namespace Events.Infrastructure;

internal class InMemoryEventStore
{
	private readonly List<Event> _events = [];

	public IReadOnlyCollection<Event> GetAll()
	{
		return _events;
	}

	public Event? Find(Guid eventId)
	{
		return _events.Find(e => e.Id == eventId);
	}

	public void Add(Event @event)
	{
		_events.Add(@event);
	}

	public void Delete(Event @event)
	{
		_events.Remove(@event);
	}

	public bool Exists(Guid eventId)
	{
		return _events.Any(e => e.Id == eventId);
	}
}