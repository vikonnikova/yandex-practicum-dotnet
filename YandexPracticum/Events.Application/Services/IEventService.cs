using Events.Application.UseCases.Dto;

namespace Events.Application.UseCases;

public interface IEventService
{
	PaginatedResult<EventInfoDto> GetBy(Filters filters, int page, int pageSize);

	EventInfoDto GetById(Guid eventId);

	EventInfoDto Add(EventDto eventData);

	void Update(EventDto eventData);

	void Remove(Guid eventId);
}