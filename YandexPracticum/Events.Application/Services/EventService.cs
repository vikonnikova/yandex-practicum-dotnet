using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.Services.Dto;
using Events.Domain;

namespace Events.Application.Services;

public class EventService(IEventRepository repository) : IEventService
{
	public async Task<PaginatedResult<EventInfoDto>> GetBy(int page, int pageSize, Filters filters,
		CancellationToken cancellationToken)
	{
		var result = await repository.GetFiltered(page, pageSize, filters, cancellationToken);

		return new PaginatedResult<EventInfoDto>(result.TotalItems, page, result.Data.Count,
			result.Data.Select(x => x.ToDto()).ToArray());
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

		repository.Add(@event);

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

		repository.Delete(eventToDelete);

		await repository.SaveChangesAsync(cancellationToken);
	}
}