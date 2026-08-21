using Events.Domain;

namespace Events.Application.Contracts;

public record AddUserCommand(string Login, string PasswordHash, UserRole Role);