using Events.Api.Contracts;
using Events.Application;
using Events.Application.Contracts.Commands;
using Events.Application.Contracts.Queries;
using Events.Domain;
using Shared.Contracts;

namespace Events.Api.Mappings;

internal static class EventMapping
{
    internal static CreateEventCommand ToAddCommand(this EventRequest data)
    {
        return new CreateEventCommand(data.Title, data.Description, data.StartAt, data.EndAt, data.TotalSeats);
    }

    internal static UpdateEventCommand ToUpdateCommand(this EventRequest data, Guid eventId)
    {
        return new UpdateEventCommand(eventId, data.Title, data.Description, data.StartAt, data.EndAt, data.TotalSeats);
    }

    internal static EventResponse ToResponse(this Event @event)
    {
        return new EventResponse(@event.Id, @event.Title, @event.Description, @event.Period.StartAt,
            @event.Period.EndAt, @event.TotalSeats, @event.AvailableSeats);
    }

    internal static EventResponse[] ToResponse(this IReadOnlyList<Event> events)
    {
        return events.Select(x => x.ToResponse()).ToArray();
    }

    internal static GetEventsByQuery ToQuery(this GetEventsQuery data)
    {
        return new GetEventsByQuery(data.Page, data.PageSize, new Filters(data.Title, data.From, data.To));
    }

    internal static PaginatedResult<EventResponse> ToResponse(
        this PaginatedResult<Event> paginatedEvents)
    {
        var data = paginatedEvents.Data.Select(x =>
            new EventResponse(x.Id, x.Title, x.Description, x.Period.StartAt, x.Period.EndAt, x.TotalSeats,
                x.AvailableSeats)).ToArray();
        
        return new PaginatedResult<EventResponse>(data, paginatedEvents.TotalItems);
    }
}