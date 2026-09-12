using Events.Domain;

namespace Events.Application.QueryHandlers;

public sealed record CachedEvent(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt,
    int TotalSeats,
    int AvailableSeats)
{
    public static CachedEvent FromDomain(Event @event)
    {
        return new CachedEvent(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.Period.StartAt,
            @event.Period.EndAt,
            @event.TotalSeats,
            @event.AvailableSeats);
    }

    public Event ToDomain()
    {
        return Event.Restore(
            Id,
            Title,
            Description,
            EventPeriod.Create(StartAt, EndAt),
            TotalSeats,
            AvailableSeats);
    }
}