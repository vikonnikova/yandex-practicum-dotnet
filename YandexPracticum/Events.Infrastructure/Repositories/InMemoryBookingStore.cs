using Events.Application.Interfaces;
using Events.Domain;

namespace Events.Infrastructure;

public class InMemoryBookingStore : IBookingRepository
{
	private readonly List<Booking> _bookings = [];
	
	public Booking? Find(Guid bookingId)
	{
		return _bookings.Find(b => b.Id == bookingId);
	}

	public void Add(Booking booking)
	{
		_bookings.Add(booking);
	}
}