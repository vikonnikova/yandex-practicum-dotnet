using Events.Application.Exceptions;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class EventService : IEventService
{
	private readonly List<Event> _events = [];

	public PaginatedResult<EventDto> GetBy(string? title, DateTime? from, DateTime? to, int page, int pageSize)
	{
		IEnumerable<Event> filteredEvents = _events;

		if (title != null)
		{
			filteredEvents = filteredEvents.Where(x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
		}

		if (from.HasValue)
		{
			filteredEvents = filteredEvents.Where(x => x.Period.StartAt >= from);
		}

		if (to.HasValue)
		{
			filteredEvents = filteredEvents.Where(x => x.Period.EndAt <= to);
		}

		var totalItems = filteredEvents.Count();
		var result = filteredEvents.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.ToDto()).ToArray();

		return new PaginatedResult<EventDto>(totalItems, page, result.Length, result);
	}

	public EventDto GetById(int eventId)
	{
		var @event = _events.Find(e => e.Id == eventId);

		return @event?.ToDto() ?? throw new EntityNotFoundException("Событие", eventId);
	}

	public EventDto Add(EventDto eventData)
	{
		var @event = Event.Create(eventData.Id, eventData.Title, eventData.Description,
			EventPeriod.Create(eventData.StartAt, eventData.EndAt));

		_events.Add(@event);

		return @event.ToDto();
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