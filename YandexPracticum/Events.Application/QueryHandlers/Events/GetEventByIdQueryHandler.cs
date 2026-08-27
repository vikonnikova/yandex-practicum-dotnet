using Events.Application.Contracts.Queries.Events;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.QueryHandlers.Events;

internal class GetEventByIdQueryHandler(IEventRepository eventRepository) : IRequestHandler<GetEventByIdQuery, Event>
{
    public async Task<Event> Handle(GetEventByIdQuery query, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.Find(query.EventId, cancellationToken);

        return @event ?? throw new EntityNotFoundException("Событие", query.EventId);
    }
}