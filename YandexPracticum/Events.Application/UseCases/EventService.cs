using Events.Application.Dto;
using Events.Application.Mappings;
using Events.Domain;

namespace Events.Application.UseCases;

public class EventService : IEventService
{
	private readonly List<Event> _events = [];

	public IReadOnlyCollection<EventResponse> GetAll()
	{
		return _events.Select(e => e.ToDto()).ToArray();
	}

	public EventResponse? GetById(int eventId)
	{
		return _events.Find(e => e.Id == eventId)?.ToDto();
	}

	public void Add(CreateEventRequest eventRequest)
	{
		_events.Add(Event.Create(eventRequest.Id, eventRequest.Title, eventRequest.Description,
			EventPeriod.Create(eventRequest.StartAt, eventRequest.EndAt)));
	}

	public void Update(int eventId, UpdateEventRequest eventRequest)
	{
		var eventToUpdate = _events.Find(e => e.Id == eventId);

		if (eventToUpdate is null)
		{
			throw new Exception("Event with id not found");
		}

		eventToUpdate.Update(eventRequest.Title, eventRequest.Description,
			EventPeriod.Create(eventRequest.StartAt, eventRequest.EndAt));
	}

	public void Remove(int eventId)
	{
		var eventToDelete = _events.Find(e => e.Id == eventId);

		if (eventToDelete is null)
		{
			throw new Exception("Event with id not found");
		}

		_events.Remove(eventToDelete);
	}
}