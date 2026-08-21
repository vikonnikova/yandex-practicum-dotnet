using Events.Api.Contracts.Users;
using Events.Application.Contracts.Commands.Users;

namespace Events.Api.Mappings;

internal static class UserMapping
{
	internal static AddUserCommand ToCommand(this UserRequest data)
	{
		return new AddUserCommand(data.Login, data.Password, MapRole(data.Role));
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