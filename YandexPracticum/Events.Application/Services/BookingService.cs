using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.Services.Dto;
using Events.Domain;

namespace Events.Application.Services;

public class BookingService(IBookingRepository repository, IEventRepository eventRepository) : IBookingService
{
	private static readonly SemaphoreSlim AdditionSemaphore = new(1, 1);

	public async Task<BookingDto> GetById(Guid bookingId, CancellationToken cancellationToken)
	{
		var booking = await repository.Find(bookingId, cancellationToken);

		return booking?.ToDto() ?? throw new EntityNotFoundException("Бронь", bookingId);
	}

	public async Task<BookingDto> Add(BookingToAddDto bookingData, CancellationToken cancellationToken)
	{
		Booking booking;

		await AdditionSemaphore.WaitAsync(cancellationToken);
		try
		{
			var @event = await eventRepository.Find(bookingData.EventId, cancellationToken);

			if (@event is null)
			{
				throw new EntityNotFoundException("Событие", bookingData.EventId);
			}

			var seatsExist = @event.TryReserveSeats();

			if (!seatsExist)
			{
				throw new NoAvailableSeatsException();
			}

			booking = Booking.Create(Guid.NewGuid(), bookingData.EventId, DateTime.UtcNow);
			repository.Add(booking, cancellationToken);
			
			await repository.SaveChangesAsync(cancellationToken);
		}
		finally
		{
			AdditionSemaphore.Release();
		}

		return booking.ToDto();
	}
}