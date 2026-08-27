namespace Events.Api.Contracts.Auth;

/// <summary>
/// Представляет данные для входа в систему.
/// </summary>
/// <param name="Login">Логин пользователя.</param>
/// <param name="Password">Пароль пользователя.</param>
public record LoginRequest(string Login, string Password);