using Events.Application.Contracts.Commands.Bookings;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Domain.Exceptions;
using MediatR;

namespace Events.Application.UseCases.Bookings;

public class CancelBookingCommandHandler(
    TimeProvider timeProvider,
    ICurrentUserContext userContext,
    IEventRepository eventRepository,
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

        var @event = await eventRepository.Find(booking.EventId, cancellationToken);

        if (@event is null)
        {
            throw new EntityNotFoundException("Событие", booking.EventId);
        }

        if (!userContext.IsAdmin && booking.UserId != userContext.UserId)
        {
            throw new AccessDeniedException("Недостаточно прав.");
        }

        if (@event.Period.StartAt < timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new PastEventCancellationException();
        }

        booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);

        await bookingRepository.SaveChangesAsync(cancellationToken);
    }
}