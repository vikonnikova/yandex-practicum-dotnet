using Events.Application.Exceptions;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class EventService : IEventService
{
	private readonly List<Event> _events = [];

	public IReadOnlyCollection<Event> GetAll()
	{
		return _events.ToArray();
	}

	public Event? GetById(int eventId)
	{
		var @event = _events.Find(e => e.Id == eventId);

		return @event ?? throw new EntityNotFoundException("Событие", eventId);
	}

	public void Add(EventDto eventData)
	{
		_events.Add(Event.Create(eventData.Id, eventData.Title, eventData.Description,
			EventPeriod.Create(eventData.StartAt, eventData.EndAt)));
	}

	public void Update(EventDto eventData)
	{
		var eventToUpdate = _events.Find(e => e.Id == eventData.Id);

		if (eventToUpdate is null)
		{
			throw new EntityNotFoundException("Событие", eventData.Id);
		}

		eventToUpdate.Update(eventData.Title, eventData.Description,
			EventPeriod.Create(eventData.StartAt, eventData.EndAt));
	}

	public void Remove(int eventId)
	{
		var eventToDelete = _events.Find(e => e.Id == eventId);

		if (eventToDelete is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		_events.Remove(eventToDelete);
	}
}