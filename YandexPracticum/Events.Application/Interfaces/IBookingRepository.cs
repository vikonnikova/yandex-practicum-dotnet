using Events.Domain;

namespace Events.Application.Interfaces;

public interface IBookingRepository
{
	Task<Booking?> Find(Guid bookingId, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<Guid>> GetPending(CancellationToken cancellationToken);

	void Add(Booking booking, CancellationToken cancellationToken);

	Task SaveChangesAsync(CancellationToken cancellationToken);
}