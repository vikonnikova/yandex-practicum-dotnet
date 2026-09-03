using Bookings.Application.Contracts.Queries;
using Bookings.Application.Interfaces;
using MediatR;
using Shared.Contracts;

namespace Bookings.Application.QueryHandlers;

internal class GetBookingsByUserQueryHandler(ICurrentUserContext userContext, IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingsByUserQuery, PaginatedResult<Domain.Booking>>
{
    public async Task<PaginatedResult<Domain.Booking>> Handle(GetBookingsByUserQuery query,
        CancellationToken cancellationToken)
    {
        return await bookingRepository.GetByUser(userContext.UserId, query.Page, query.PageSize, cancellationToken);
    }
}