namespace Events.Api.Contracts;

/// <summary>
/// Представляет данные для создания брони.
/// </summary>
/// <param name="UserId">Идентификатор пользователя.</param>
public record BookingRequest(Guid UserId);