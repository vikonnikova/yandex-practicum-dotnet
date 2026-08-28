using Auth.Api.Contracts.Auth;
using Auth.Api.Contracts.Users;
using Auth.Application.Contracts.Auth;

namespace Auth.Api.Mapping;

internal static class AuthMapping
{
    internal static RegisterUserCommand ToCommand(this RegistrationRequest dto)
    {
        return new RegisterUserCommand(dto.Login, dto.Password, MapRole(dto.Role));
    }

    private static Auth.Domain.UserRole MapRole(UserRole value)
    {
        return value switch
        {
            UserRole.User => Domain.UserRole.User,
            UserRole.Admin => Domain.UserRole.Admin,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}