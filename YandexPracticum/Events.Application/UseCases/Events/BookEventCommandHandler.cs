using Events.Application.Contracts.Commands.Events;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Domain;
using Events.Domain.Exceptions;
using MediatR;

namespace Events.Application.UseCases.Events;

internal class BookEventCommandHandler(
    TimeProvider timeProvider,
    ICurrentUserContext userContext,
    IBookingRepository bookingRepository,
    IEventRepository eventRepository)
    : IRequestHandler<BookEventCommand, Booking>
{
    private const int BookingLimitPerUser = 10;
    private static readonly SemaphoreSlim AdditionSemaphore = new(1, 1);

    public async Task<Booking> Handle(BookEventCommand command, CancellationToken cancellationToken)
    {
        //TODO обработчик команды взял на себя координацию инвариантов

        Booking booking;

        await AdditionSemaphore.WaitAsync(cancellationToken);
        try
        {
            var @event = await eventRepository.Find(command.EventId, cancellationToken);

            if (@event is null)
            {
                throw new EntityNotFoundException("Событие", command.EventId);
            }

            if (@event.Period.StartAt < timeProvider.GetUtcNow().UtcDateTime)
            {
                throw new PastEventBookingException();
            }

            var bookingsCount = await bookingRepository.CountPendingByUser(userContext.UserId, cancellationToken);

            if (bookingsCount >= BookingLimitPerUser)
            {
                throw new BookingLimitReachingException(
                    $"Достигнут лимит [{BookingLimitPerUser}] бронирования у события.");
            }

            var seatsExist = @event.TryReserveSeats();

            if (!seatsExist)
            {
                throw new NoAvailableSeatsException();
            }

            booking = Booking.Create(Guid.NewGuid(), command.EventId, userContext.UserId,
                timeProvider.GetUtcNow().UtcDateTime);
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