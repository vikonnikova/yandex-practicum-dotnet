using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public interface IEventService
{
	IReadOnlyCollection<EventDto> GetAll();

	EventDto GetById(int eventId);

	EventDto Add(EventDto eventData);

	void Update(EventDto eventData);

	void Remove(int eventId);
}