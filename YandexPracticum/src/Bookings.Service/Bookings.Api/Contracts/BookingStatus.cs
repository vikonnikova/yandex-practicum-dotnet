using System.Text.Json.Serialization;

namespace Bookings.Api.Contracts.Bookings;

/// <summary>
/// Представляет статусы бронирования.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
    /// <summary>
    /// Создана, ожидает обработки.
    /// </summary>
    Pending,

    /// <summary>
    /// Подтверждена.
    /// </summary>
    Confirmed,

    /// <summary>
    /// Отклонена.
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Отменена.
    /// </summary>
    Cancelled
}