using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Queries;

public record GetTopEventsQuery : IRequest<IReadOnlyCollection<Event>>;
