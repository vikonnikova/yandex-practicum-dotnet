using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Queries.Users;

public record GetUserByIdQuery(Guid UserId) : IRequest<User>;