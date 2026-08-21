using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Commands.Bookings;

public record BookEventCommand(Guid EventId, Guid UserId) : IRequest<Booking>;