using Events.Api.Contracts;
using Events.Api.Contracts.Events;
using Events.Application;
using Events.Application.Contracts.Commands;
using Events.Application.Contracts.Queries;
using Events.Domain;

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

    internal static GetEventsByQuery ToQuery(this GetEventsQuery data)
    {
        return new GetEventsByQuery(data.Page, data.PageSize, new Filters(data.Title, data.From, data.To));
    }

    internal static PaginatedResult<EventResponse> ToPaginatedResponse(
        this Application.Contracts.PaginatedResult<Event> paginatedEvents)
    {
        return new PaginatedResult<EventResponse>(paginatedEvents.ToResponse(), paginatedEvents.ToMetadata());
    }

    private static IReadOnlyCollection<EventResponse> ToResponse(
        this Application.Contracts.PaginatedResult<Event> paginatedEvents)
    {
        return paginatedEvents.Items.Select(x =>
                new EventResponse(x.Id, x.Title, x.Description, x.Period.StartAt, x.Period.EndAt, x.TotalSeats,
                    x.AvailableSeats))
            .ToArray();
    }

    private static Metadata ToMetadata(this Application.Contracts.PaginatedResult<Event> paginatedEvents)
    {
        return new Metadata(paginatedEvents.TotalItems, paginatedEvents.CurrentPage, paginatedEvents.ItemsPerPage);
    }
}