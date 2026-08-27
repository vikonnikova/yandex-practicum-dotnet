using Events.Domain;
using MediatR;

namespace Events.Application.Contracts.Auth;

public record RegisterUserCommand(string Login, string Password, UserRole Role) : IRequest;