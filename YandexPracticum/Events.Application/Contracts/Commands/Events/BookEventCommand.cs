using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Commands.Events;

public record BookEventCommand(Guid EventId) : IRequest<Booking>;