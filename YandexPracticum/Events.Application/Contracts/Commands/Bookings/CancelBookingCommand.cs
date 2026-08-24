using MediatR;

namespace Events.Application.Contracts.Commands.Bookings;

public record CancelBookingCommand(Guid BookingId) : IRequest;