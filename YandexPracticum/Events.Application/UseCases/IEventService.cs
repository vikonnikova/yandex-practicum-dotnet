using Events.Application.Dto;

namespace Events.Application.UseCases;

public interface IEventService
{
	IReadOnlyCollection<EventResponse> GetAll();

	EventResponse? GetById(int eventId);

	void Add(CreateEventRequest eventRequest);

	void Update(int eventId, UpdateEventRequest eventRequest);

	void Remove(int eventId);
}