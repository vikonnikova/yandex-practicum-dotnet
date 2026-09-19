namespace Auth.Api.Contracts.Auth;

/// <summary>
/// Представляет данные для смены пароля.
/// </summary>
/// <param name="CurrentPassword">Текущий пароль.</param>
/// <param name="NewPassword">Новый пароль.</param>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
