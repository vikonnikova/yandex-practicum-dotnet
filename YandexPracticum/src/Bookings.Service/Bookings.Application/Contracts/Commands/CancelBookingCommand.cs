using MediatR;

namespace Bookings.Application.Contracts.Commands;

public record CancelBookingCommand(Guid BookingId) : IRequest;