using Bookings.Application.Contracts.Queries;
using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces;
using Bookings.Domain;
using MediatR;

namespace Bookings.Application.QueryHandlers;

internal class GetBookingByIdQueryHandler(ICurrentUserContext userContext, IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingByIdQuery, Booking>
{
    public async Task<Booking> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.Find(query.BookingId, cancellationToken);

        if (booking is null)
        {
            throw new EntityNotFoundException("Бронь", query.BookingId);
        }

        if (booking.UserId != userContext.UserId)
        {
            throw new AccessDeniedException("Недостаточно прав.");
        }

        return booking;
    }
}