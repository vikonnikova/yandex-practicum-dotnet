using Events.Domain;

namespace Events.Application.Interfaces;

public interface IBookingRepository
{
	Task<Booking?> Find(Guid bookingId, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<Booking>> GetPending(CancellationToken cancellationToken);

	Task Add(Booking booking, CancellationToken cancellationToken);
}