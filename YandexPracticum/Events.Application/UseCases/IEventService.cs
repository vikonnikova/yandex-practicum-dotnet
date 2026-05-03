using Events.Domain;

namespace Events.Application.UseCases;

public interface IEventService
{
	IReadOnlyCollection<Event> GetAll();
	
	void Add(Event @event);
	
	void Update(Event @event);
	
	void Remove(Event @event);
}