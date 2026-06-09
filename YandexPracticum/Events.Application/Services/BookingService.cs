using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class BookingService(IBookingRepository repository) : IBookingService
{
	public BookingDto GetById(Guid bookingId)
	{
		var booking = repository.Find(bookingId);

		return booking?.ToDto() ?? throw new EntityNotFoundException("Бронь", bookingId);
	}

	public BookingDto Add(BookingToAddDto bookingData)
	{
		var booking = Booking.Create(bookingData.Id, bookingData.EventId);
		repository.Add(booking);

		return booking.ToDto();
	}
}