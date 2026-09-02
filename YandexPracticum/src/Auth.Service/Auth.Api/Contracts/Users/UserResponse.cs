namespace Auth.Api.Contracts.Users;

/// <summary>
/// Представляет данные пользователя.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Login">Логин.</param>
/// <param name="Role">Роль в системе.</param>
public record UserResponse(Guid Id, string Login, UserRole Role);