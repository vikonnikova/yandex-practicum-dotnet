using Events.Domain;

namespace Events.Application.Interfaces;

public interface IBookingRepository
{
	Booking? Find(Guid bookingId);

	void Add(Booking booking);
}