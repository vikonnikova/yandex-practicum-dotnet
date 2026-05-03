using Events.Application.Dto;

namespace Events.Application.UseCases;

public interface IEventService
{
	IReadOnlyCollection<EventData> GetAll();

	EventData? GetById(int eventId);

	void Add(CreateEventData eventData);

	void Update(int eventId, UpdateEventData eventData);

	void Remove(int eventId);
}