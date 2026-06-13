using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class EventService(IEventRepository repository) : IEventService
{
	public PaginatedResult<EventDto> GetBy(Filters filters, int page, int pageSize)
	{
		IEnumerable<Event> filteredEvents = repository.GetAll(); // TODO перенести в репозиторий

		if (filters.Title != null)
		{
			filteredEvents = filteredEvents.Where(x => x.Title.Contains(filters.Title, StringComparison.OrdinalIgnoreCase));
		}

		if (filters.From.HasValue)
		{
			filteredEvents = filteredEvents.Where(x => x.Period.StartAt >= filters.From);
		}

		if (filters.To.HasValue)
		{
			filteredEvents = filteredEvents.Where(x => x.Period.EndAt <= filters.To);
		}

		var totalItems = filteredEvents.Count();
		var result = filteredEvents.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.ToDto()).ToArray();

		return new PaginatedResult<EventDto>(totalItems, page, result.Length, result);
	}

	public EventDto GetById(Guid eventId)
	{
		var @event = repository.Find(eventId);

		return @event?.ToDto() ?? throw new EntityNotFoundException("Событие", eventId);
	}

	public EventDto Add(EventDto eventData)
	{
		var @event = Event.Create(eventData.Id, eventData.Title, eventData.Description,
			EventPeriod.Create(eventData.StartAt, eventData.EndAt));

		repository.Add(@event);

		return @event.ToDto();
	}

	public void Update(EventDto eventData)
	{
		var eventToUpdate = repository.Find(eventData.Id);

		if (eventToUpdate is null)
		{
			throw new EntityNotFoundException("Событие", eventData.Id);
		}

		eventToUpdate.Update(eventData.Title, eventData.Description,
			EventPeriod.Create(eventData.StartAt, eventData.EndAt));
	}

	public void Remove(Guid eventId)
	{
		var eventToDelete = repository.Find(eventId);

		if (eventToDelete is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		repository.Delete(eventToDelete);
	}
}