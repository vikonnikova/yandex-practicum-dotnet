using Events.Application.Dto;
using Events.Application.Mappings;
using Events.Domain;

namespace Events.Application.UseCases;

public class EventService : IEventService
{
	private readonly List<Event> _events = [];

	public IReadOnlyCollection<EventData> GetAll()
	{
		return _events.Select(e => e.ToDto()).ToArray();
	}

	public EventData? GetById(int eventId)
	{
		return _events.Find(e => e.Id == eventId)?.ToDto();
	}

	public void Add(CreateEventData eventData)
	{
		_events.Add(Event.Create(eventData.Id, eventData.Title, eventData.Description, eventData.StartAt,
			eventData.EndAt));
	}

	public void Update(int eventId, UpdateEventData eventData)
	{
		var eventToUpdate = _events.Find(e => e.Id == eventId);

		if (eventToUpdate is null)
		{
			throw new Exception("Event with id not found");
		}

		eventToUpdate.Update(eventData.Title, eventData.Description, eventData.StartAt, eventToUpdate.EndAt);
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