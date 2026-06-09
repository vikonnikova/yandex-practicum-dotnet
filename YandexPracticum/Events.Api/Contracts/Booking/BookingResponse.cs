namespace Events.Api.Contracts;

/// <summary>
/// Представляет данные бронирования.
/// </summary>
/// <param name="Status">Статус.</param>
public record BookingResponse(BookingStatus Status);