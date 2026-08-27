using Auth.Domain;
using MediatR;

namespace Auth.Application.Contracts.Auth;

public record RegisterUserCommand(string Login, string Password, UserRole Role) : IRequest;