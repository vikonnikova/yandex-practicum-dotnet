using Events.Application.Services.Dto;

namespace Events.Application.Services;

public interface IEventService
{
	Task<PaginatedResult<EventInfoDto>> GetBy(int page, int pageSize, Filters filters,
		CancellationToken cancellationToken);

	Task<EventInfoDto> GetById(Guid eventId, CancellationToken cancellationToken);

	Task<EventInfoDto> Add(EventDto eventData, CancellationToken cancellationToken);

	Task Update(EventToUpdateDto eventData, CancellationToken cancellationToken);

	Task Remove(Guid eventId, CancellationToken cancellationToken);
}