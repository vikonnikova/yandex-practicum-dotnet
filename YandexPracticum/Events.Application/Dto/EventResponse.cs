namespace Events.Application.Dto;

/// <summary>
/// Представляет данные события.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Title">Наименование.</param>
/// <param name="Description">Описание.</param>
/// <param name="StartAt">Дата начала.</param>
/// <param name="EndAt">Дата окончания.</param>
public record EventResponse(int Id, string Title, string? Description, DateTime StartAt, DateTime EndAt);