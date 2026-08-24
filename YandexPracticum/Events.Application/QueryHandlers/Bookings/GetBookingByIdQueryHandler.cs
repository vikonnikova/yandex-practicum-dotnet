using Events.Application.Contracts.Queries.Bookings;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.QueryHandlers.Bookings;

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