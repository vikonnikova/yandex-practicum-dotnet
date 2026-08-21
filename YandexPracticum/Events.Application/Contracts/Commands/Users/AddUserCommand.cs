using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Commands.Users;

public record AddUserCommand(string Login, string PasswordHash, UserRole Role) : IRequest<Guid>;