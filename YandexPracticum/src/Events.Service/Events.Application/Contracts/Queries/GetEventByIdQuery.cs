using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Queries;

public record GetEventByIdQuery(Guid EventId) : IRequest<Event>;