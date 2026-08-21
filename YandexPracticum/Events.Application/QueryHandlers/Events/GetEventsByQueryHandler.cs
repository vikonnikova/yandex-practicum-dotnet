using Events.Application.Contracts;
using Events.Application.Contracts.Queries.Events;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.QueryHandlers.Events;

internal class GetEventsByQueryHandler(IEventRepository eventRepository)
	: IRequestHandler<GetEventsByQuery, PaginatedResult<Event>>
{
	public async Task<PaginatedResult<Event>> Handle(GetEventsByQuery query, CancellationToken cancellationToken)
	{
		var result = await eventRepository.GetFiltered(query.Page, query.PageSize, query.Filters, cancellationToken);

		return new PaginatedResult<Event>(result.TotalItems, query.Page, result.Data.Count, result.Data.ToArray());
	}
}