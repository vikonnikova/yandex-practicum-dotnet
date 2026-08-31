namespace Shared.Contracts;

/// <summary>
/// Контракт события подтверждения бронирования.
/// </summary>
/// <param name="BookingId">Идентификатор брони.</param>
/// <param name="EventId">Идентификатор события.</param>
/// <param name="UserId">Идентификатор пользователя, совершившего бронь.</param>
/// <param name="SeatsCount">Количество забронированных мест.</param>
/// <param name="ConfirmedAt">Время подтверждения бронирования.</param>
public record BookingConfirmedEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    int SeatsCount,
    DateTime ConfirmedAt);