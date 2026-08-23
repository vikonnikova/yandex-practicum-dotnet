using Events.Api.Contracts.Users;

namespace Events.Api.Contracts.Auth;

/// <summary>
/// Представляет данные для регистрации пользователя в системе.
/// </summary>
/// <param name="Login">Логин пользователя.</param>
/// <param name="Password">Пароль пользователя.</param>
/// <param name="Role">Роль пользователя.</param>
public record RegistrationRequest(string Login, string Password, UserRole Role = UserRole.User);