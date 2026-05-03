using Events.Domain;

namespace Events.Application.UseCases;

public class EventService : IEventService
{
	private readonly List<Event> _events = [];
	
	public IReadOnlyCollection<Event> GetAll()
	{
		return _events;
	}
	
	public void Add(Event @event)
	{
		_events.Add(@event);
	}
	
	public void Update(Event @event)
	{
		var eventToUpdate = _events.Find(e => e.Id == @event.Id);

		if (eventToUpdate is null)
		{
			throw new Exception("Event with id not found");
		}
		
		_events.Remove(eventToUpdate);
		_events.Add(@event);
	}
	
	public void Remove(Event @event)
	{
		_events.Remove(@event);
	}
}