using Events.Application.Contracts.Commands.Bookings;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using MediatR;

namespace Events.Application.UseCases.Bookings;

public class RemoveBookingCommandHandler(TimeProvider timeProvider, ICurrentUserContext userContext, IBookingRepository bookingRepository)
	: IRequestHandler<RemoveBookingCommand>
{
	public async Task Handle(RemoveBookingCommand command, CancellationToken cancellationToken)
	{
		var booking = await bookingRepository.Find(command.BookingId, cancellationToken);

		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", command.BookingId);
		}

		if (booking.UserId != userContext.UserId) // TODO тут или не тут пользователя доставать, подумать
		{
			throw new AccessDeniedException("Недостаточно прав.");
		}

		booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);

		await bookingRepository.SaveChangesAsync(cancellationToken);
	}
}