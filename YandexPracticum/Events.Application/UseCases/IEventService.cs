using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public interface IEventService
{
	IReadOnlyCollection<Event> GetAll();

	Event? GetById(int eventId);

	void Add(EventDto eventData);

	void Update(EventDto eventData);

	void Remove(int eventId);
}