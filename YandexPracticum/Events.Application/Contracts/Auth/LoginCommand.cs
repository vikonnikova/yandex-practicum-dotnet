using MediatR;

namespace Events.Application.Contracts.Auth;

public record LoginCommand(string Login, string Password) : IRequest<string>;