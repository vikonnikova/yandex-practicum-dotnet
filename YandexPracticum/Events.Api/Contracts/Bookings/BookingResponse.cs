namespace Events.Api.Contracts.Bookings;

/// <summary>
/// Представляет данные бронирования.
/// </summary>
/// <param name="BookingId">Идентификатор брони.</param>
/// <param name="EventId">Идентификатор события.</param>
/// <param name="Status">Статус.</param>
public record BookingResponse(Guid BookingId, Guid EventId, BookingStatus Status);