using Events.Domain;

namespace Events.Application.Interfaces;

public interface IBookingRepository
{
	Booking? Find(Guid bookingId);

	IReadOnlyCollection<Booking> GetPending();

	void Add(Booking booking);
}