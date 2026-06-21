using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class BookingService(IBookingRepository repository, IEventRepository eventRepository) : IBookingService
{
	private readonly Lock _bookingLock = new();

	public BookingDto GetById(Guid bookingId)
	{
		var booking = repository.Find(bookingId);

		return booking?.ToDto() ?? throw new EntityNotFoundException("Бронь", bookingId);
	}

	public BookingDto Add(BookingToAddDto bookingData)
	{
		Booking booking;

		lock (_bookingLock)
		{
			var @event = eventRepository.Find(bookingData.EventId);

			if (@event is null)
			{
				throw new EntityNotFoundException("Событие", bookingData.EventId);
			}

			var seatsExist = @event.TryReserveSeats();

			if (!seatsExist)
			{
				throw new NoAvailableSeatsException();
			}

			booking = Booking.Create(bookingData.BookingId, bookingData.EventId, DateTime.UtcNow);
			repository.Add(booking);
		}

		return booking.ToDto();
	}
}