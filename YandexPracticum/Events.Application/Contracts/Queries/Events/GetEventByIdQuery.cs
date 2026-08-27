using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Queries.Events;

public record GetEventByIdQuery(Guid EventId) : IRequest<Event>;