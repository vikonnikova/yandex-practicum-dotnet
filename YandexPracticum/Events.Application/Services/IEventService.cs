using Events.Application.UseCases.Dto;

namespace Events.Application.UseCases;

public interface IEventService
{
	PaginatedResult<EventDto> GetBy(Filters filters, int page, int pageSize);

	EventDto GetById(Guid eventId);

	EventDto Add(EventDto eventData);

	void Update(EventDto eventData);

	void Remove(Guid eventId);
}