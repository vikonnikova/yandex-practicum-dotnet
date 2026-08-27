using MediatR;

namespace Events.Application.Contracts.Commands;

public record DeleteEventCommand(Guid EventId) : IRequest;