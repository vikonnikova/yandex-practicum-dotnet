namespace Auth.Api.Contracts.Users;

/// <summary>
/// Представляет данные для создания/изменения пользователя.
/// </summary>
/// <param name="Login">Логин пользователя.</param>
/// <param name="Password">Пароль.</param>
/// <param name="Role">Роль в системе.</param>
public record UserRequest(string Login, string Password, UserRole Role);