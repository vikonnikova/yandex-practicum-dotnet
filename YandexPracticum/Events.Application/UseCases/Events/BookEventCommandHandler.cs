using Events.Application.Contracts.Commands.Bookings;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Domain;
using Events.Domain.Exceptions;
using MediatR;

namespace Events.Application.UseCases.Events;

internal class BookEventCommandHandler(IBookingRepository bookingRepository, IEventRepository eventRepository)
	: IRequestHandler<BookEventCommand, Booking>
{
	private static readonly SemaphoreSlim AdditionSemaphore = new(1, 1);

	public async Task<Booking> Handle(BookEventCommand command, CancellationToken cancellationToken)
	{
		Booking booking;

		await AdditionSemaphore.WaitAsync(cancellationToken);
		try
		{
			var @event = await eventRepository.Find(command.EventId, cancellationToken);

			if (@event is null)
			{
				throw new EntityNotFoundException("Событие", command.EventId);
			}

			var seatsExist = @event.TryReserveSeats();

			if (!seatsExist)
			{
				throw new NoAvailableSeatsException();
			}

			booking = Booking.Create(Guid.NewGuid(), command.EventId, command.UserId, DateTime.UtcNow);
			bookingRepository.Add(booking);

			await bookingRepository.SaveChangesAsync(cancellationToken);
		}
		finally
		{
			AdditionSemaphore.Release();
		}

		return booking;
	}
}