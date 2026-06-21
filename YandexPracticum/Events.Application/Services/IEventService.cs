using Events.Application.UseCases.Dto;

namespace Events.Application.UseCases;

public interface IEventService
{
	Task<PaginatedResult<EventInfoDto>> GetBy(Filters filters, int page, int pageSize, CancellationToken cancellationToken);

	Task<EventInfoDto> GetById(Guid eventId, CancellationToken cancellationToken);

	Task<EventInfoDto> Add(EventDto eventData, CancellationToken cancellationToken);

	Task Update(EventDto eventData, CancellationToken cancellationToken);

	Task Remove(Guid eventId, CancellationToken cancellationToken);
}