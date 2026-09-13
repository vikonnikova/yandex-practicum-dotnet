using Events.Application.Contracts.Queries;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Settings;
using Events.Domain;
using MediatR;
using Shared.Settings;

namespace Events.Application.QueryHandlers;

internal class GetEventByIdQueryHandler(
    IEventRepository eventRepository,
    ICacheService cache,
    CacheSettings cacheSettings) : IRequestHandler<GetEventByIdQuery, Event>
{
    public async Task<Event> Handle(GetEventByIdQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Event(query.EventId);
        var cachedEvent = await cache.GetAsync<CachedEvent>(cacheKey, cancellationToken);
        if (cachedEvent is not null)
        {
            return cachedEvent.ToDomain();
        }

        var @event = await eventRepository.Find(query.EventId, cancellationToken)
                     ?? throw new EntityNotFoundException("Событие", query.EventId);

        await cache.SetAsync(
            cacheKey,
            CachedEvent.FromDomain(@event),
            TimeSpan.FromSeconds(cacheSettings.EventTtlSeconds),
            cancellationToken);

        return @event;
    }
}