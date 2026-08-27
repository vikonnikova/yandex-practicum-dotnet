using Auth.Domain;
using MediatR;

namespace Auth.Application.Contracts.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<User>;