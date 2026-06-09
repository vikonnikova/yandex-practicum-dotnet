using Events.Application.Exceptions;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class BookingService : IBookingService
{
	private readonly List<Booking> _bookings = [];

	public BookingDto GetById(Guid bookingId)
	{
		var booking = _bookings.Find(b => b.Id == bookingId);

		return booking?.ToDto() ?? throw new EntityNotFoundException("Бронь", bookingId);
	}
}