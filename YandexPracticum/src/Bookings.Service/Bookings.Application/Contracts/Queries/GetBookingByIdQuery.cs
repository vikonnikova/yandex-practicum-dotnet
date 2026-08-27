using Bookings.Domain;
using MediatR;

namespace Bookings.Application.Contracts.Queries;

public record GetBookingByIdQuery(Guid BookingId) : IRequest<Booking>;