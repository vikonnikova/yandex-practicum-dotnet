using Events.Api.Contracts.Users;
using Events.Domain;
using UserRole = Events.Api.Contracts.Users.UserRole;

namespace Events.Api.Mappings;

internal static class UserMapping
{
    internal static UserResponse ToResponse(this User data)
    {
        return new UserResponse(data.Id, data.Login, MapRole(data.Role));
    }

    private static Events.Domain.UserRole MapRole(UserRole value)
    {
        return value switch
        {
            UserRole.User => Domain.UserRole.User,
            UserRole.Admin => Domain.UserRole.Admin,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }

    private static UserRole MapRole(Events.Domain.UserRole value)
    {
        return value switch
        {
            Domain.UserRole.User => UserRole.User,
            Domain.UserRole.Admin => UserRole.Admin,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}