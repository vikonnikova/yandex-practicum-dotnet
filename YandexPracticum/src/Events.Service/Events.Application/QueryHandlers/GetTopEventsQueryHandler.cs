using Events.Application;
using Events.Application.Contracts.Queries;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;
using Shared.Settings;

namespace Events.Application.QueryHandlers;

internal class GetTopEventsQueryHandler(
    IEventRepository eventRepository,
    ICacheService cache,
    CacheSettings cacheSettings) : IRequestHandler<GetTopEventsQuery, IReadOnlyCollection<Event>>
{
    public async Task<IReadOnlyCollection<Event>> Handle(GetTopEventsQuery query, CancellationToken cancellationToken)
    {
    }
}