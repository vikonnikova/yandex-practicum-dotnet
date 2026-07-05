using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class EventService(IEventRepository repository) : IEventService
{
	public async Task<PaginatedResult<EventInfoDto>> GetBy(Filters filters, int page, int pageSize,
		CancellationToken cancellationToken)
	{
		IEnumerable<Event> filteredEvents = await repository.GetAll(cancellationToken); // TODO перенести в репозиторий

		if (filters.Title != null)
		{
			filteredEvents =
				filteredEvents.Where(x => x.Title.Contains(filters.Title, StringComparison.OrdinalIgnoreCase));
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

		return new PaginatedResult<EventInfoDto>(totalItems, page, result.Length, result);
	}

	public async Task<EventInfoDto> GetById(Guid eventId, CancellationToken cancellationToken)
	{
		var @event = await repository.Find(eventId, cancellationToken);

		return @event?.ToDto() ?? throw new EntityNotFoundException("Событие", eventId);
	}

	public async Task<EventInfoDto> Add(EventDto eventData, CancellationToken cancellationToken)
	{
		var @event = Event.Create(Guid.NewGuid(), eventData.Title, eventData.Description,
			EventPeriod.Create(eventData.StartAt, eventData.EndAt), eventData.TotalSeats);

		repository.Add(@event, cancellationToken);

		await repository.SaveChangesAsync(cancellationToken);

		return @event.ToDto();
	}

	public async Task Update(EventToUpdateDto eventData, CancellationToken cancellationToken)
	{
		var eventToUpdate = await repository.Find(eventData.Id, cancellationToken);

		if (eventToUpdate is null)
		{
			throw new EntityNotFoundException("Событие", eventData.Id);
		}

		eventToUpdate.Update(eventData.Title, eventData.Description,
			EventPeriod.Create(eventData.StartAt, eventData.EndAt));

		await repository.SaveChangesAsync(cancellationToken);
	}

	public async Task Remove(Guid eventId, CancellationToken cancellationToken)
	{
		var eventToDelete = await repository.Find(eventId, cancellationToken);

		if (eventToDelete is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		repository.Delete(eventToDelete, cancellationToken);

		await repository.SaveChangesAsync(cancellationToken);
	}
}