using Events.Application.Contracts.Queries;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;
using Shared.Contracts;

namespace Events.Application.QueryHandlers;

internal class GetEventsByQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventsByQuery, PaginatedResult<Event>>
{
    public async Task<PaginatedResult<Event>> Handle(GetEventsByQuery query, CancellationToken cancellationToken)
    {
        return await eventRepository.GetFiltered(query.Page, query.PageSize, query.Filters, cancellationToken);
    }
}