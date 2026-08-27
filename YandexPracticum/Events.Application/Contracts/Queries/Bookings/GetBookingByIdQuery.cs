using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Queries.Bookings;

public record GetBookingByIdQuery(Guid BookingId) : IRequest<Booking>;