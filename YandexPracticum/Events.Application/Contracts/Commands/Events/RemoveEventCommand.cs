using MediatR;

namespace Events.Application.Contracts.Commands.Events;

public record RemoveEventCommand(Guid EventId) : IRequest;