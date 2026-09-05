namespace Events.Api.Contracts;

/// <summary>
/// Представляет данные события.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Title">Наименование.</param>
/// <param name="Description">Описание.</param>
/// <param name="StartAt">Дата начала.</param>
/// <param name="EndAt">Дата окончания.</param>
/// <param name="TotalSeats">Общее количество мест.</param>
/// <param name="AvailableSeats">Количество свободных мест.</param>
public record EventResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt,
    int TotalSeats,
    int AvailableSeats);