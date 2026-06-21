using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class BookingService(IBookingRepository repository, IEventRepository eventRepository) : IBookingService
{
	private readonly SemaphoreSlim _additionSemaphore = new(1, 1);

	public async Task<BookingDto> GetById(Guid bookingId, CancellationToken cancellationToken)
	{
		var booking = await repository.Find(bookingId, cancellationToken);

		return booking?.ToDto() ?? throw new EntityNotFoundException("Бронь", bookingId);
	}

	public async Task<BookingDto> Add(BookingToAddDto bookingData, CancellationToken cancellationToken)
	{
		Booking booking;

		await _additionSemaphore.WaitAsync(cancellationToken);
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

			booking = Booking.Create(bookingData.BookingId, bookingData.EventId, DateTime.UtcNow);
			await repository.Add(booking, cancellationToken);
		}
		finally
		{
			_additionSemaphore.Release();
		}

		return booking.ToDto();
	}
}