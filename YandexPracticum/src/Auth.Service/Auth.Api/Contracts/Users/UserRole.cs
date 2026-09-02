using System.Text.Json.Serialization;

namespace Auth.Api.Contracts.Users;

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