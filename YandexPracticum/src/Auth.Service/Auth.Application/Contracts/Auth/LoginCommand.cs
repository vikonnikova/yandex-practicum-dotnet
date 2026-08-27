using MediatR;

namespace Auth.Application.Contracts.Auth;

public record LoginCommand(string Login, string Password) : IRequest<string>;