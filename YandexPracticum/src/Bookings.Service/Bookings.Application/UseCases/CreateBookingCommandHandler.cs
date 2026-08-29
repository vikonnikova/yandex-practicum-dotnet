using Bookings.Application.Contracts.Commands;
using Bookings.Application.Interfaces;
using Bookings.Domain;
using Bookings.Domain.Exceptions;
using MediatR;

namespace Bookings.Application.UseCases;

internal class CreateBookingCommandHandler(
    TimeProvider timeProvider,
    ICurrentUserContext userContext,
    IBookingRepository bookingRepository)
    : IRequestHandler<CreateBookingCommand, Booking>
{
    private const int BookingLimitPerUser = 10;
    private static readonly SemaphoreSlim AdditionSemaphore = new(1, 1);

    public async Task<Booking> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        Booking booking;

        await AdditionSemaphore.WaitAsync(cancellationToken);
        try
        {
            var bookingsCount = await bookingRepository.CountPendingByUser(userContext.UserId, cancellationToken);

            if (bookingsCount >= BookingLimitPerUser)
            {
                throw new BookingLimitReachingException(
                    $"Достигнут лимит [{BookingLimitPerUser}] бронирования.");
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