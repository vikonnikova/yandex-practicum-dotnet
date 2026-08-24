using MediatR;

namespace Events.Application.Contracts.Commands.Bookings;

public record RemoveBookingCommand(Guid BookingId) : IRequest;