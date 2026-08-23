using Events.Api.Contracts.Auth;
using Events.Api.Contracts.Users;
using Events.Application.Contracts.Auth;

namespace Events.Api.Mappings;

internal static class AuthMapping
{
	internal static RegisterUserCommand ToCommand(this RegistrationRequest dto)
	{
		return new RegisterUserCommand(dto.Login, dto.Password, MapRole(dto.Role));
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
}