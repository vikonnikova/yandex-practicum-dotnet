using Bookings.Domain;
using MediatR;
using Shared.Contracts;

namespace Bookings.Application.Contracts.Queries;

public record GetBookingsByUserQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<Booking>>;