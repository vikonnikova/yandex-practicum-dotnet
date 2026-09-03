using MediatR;

namespace Auth.Application.Contracts.Auth;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;
