using Events.Domain;

namespace Events.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> Find(Guid bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetPending(CancellationToken cancellationToken);

    Task<int> CountBy(Guid eventId, Guid userId, CancellationToken cancellationToken);

    void Add(Booking booking);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}