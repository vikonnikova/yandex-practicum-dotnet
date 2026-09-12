using Events.Application.Contracts.Queries;
using Events.Application.Interfaces;
using Events.Application.Settings;
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
        var cachedEvents = await cache.GetAsync<CachedEvent[]>(CacheKeys.Top10, cancellationToken);
        if (cachedEvents is not null)
        {
            return cachedEvents.Select(item => item.ToDomain()).ToArray();
        }

        var events = await eventRepository.GetTopBySoldPercentage(10, cancellationToken);

        await cache.SetAsync(
            CacheKeys.Top10,
            events.Select(CachedEvent.FromDomain).ToArray(),
            TimeSpan.FromSeconds(cacheSettings.TopEventsTtlSeconds),
            cancellationToken);

        return events;
    }
}