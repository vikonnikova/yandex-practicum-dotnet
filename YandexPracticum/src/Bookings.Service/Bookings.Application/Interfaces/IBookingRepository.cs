using Bookings.Domain;
using Shared.Contracts;

namespace Bookings.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> Find(Guid bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetPending(CancellationToken cancellationToken);

    Task<int> CountPendingByUser(Guid userId, CancellationToken cancellationToken);

    Task<PaginatedResult<Booking>> GetByUser(Guid userId, int page, int pageSize, CancellationToken cancellationToken);

    void Add(Booking booking);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}