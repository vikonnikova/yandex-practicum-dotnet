using Bookings.Application.Contracts.Commands;
using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces;
using MediatR;

namespace Bookings.Application.UseCases;

public class CancelBookingCommandHandler(
    TimeProvider timeProvider,
    ICurrentUserContext userContext,
    IBookingRepository bookingRepository)
    : IRequestHandler<CancelBookingCommand>
{
    public async Task Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.Find(command.BookingId, cancellationToken);

        if (booking is null)
        {
            throw new EntityNotFoundException("Бронь", command.BookingId);
        }

        if (!userContext.IsAdmin && booking.UserId != userContext.UserId)
        {
            throw new AccessDeniedException("Недостаточно прав.");
        }

        booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);

        await bookingRepository.SaveChangesAsync(cancellationToken);
    }
}