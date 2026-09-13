using Events.Domain;
using MediatR;
using Shared.Contracts;

namespace Events.Application.Contracts.Queries;

public record GetEventsByQuery(int Page = 1, int PageSize = 10, Filters? Filters = null)
    : IRequest<PaginatedResult<Event>>;