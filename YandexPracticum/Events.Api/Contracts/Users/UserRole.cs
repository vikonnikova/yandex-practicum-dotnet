using System.Text.Json.Serialization;

namespace Events.Api.Contracts.Users;

/// <summary>
/// Представляет роли пользователя.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
	/// <summary>
	/// Обычный пользователь.
	/// </summary>
	User,

	/// <summary>
	/// Администратор.
	/// </summary>
	Admin,
}