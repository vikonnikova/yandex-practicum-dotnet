using System.ComponentModel.DataAnnotations;

namespace Events.Application.Dto;

/// <summary>
/// Представляет данные для создания события.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Title">Наименование.</param>
/// <param name="Description">Описание.</param>
/// <param name="StartAt">Дата начала.</param>
/// <param name="EndAt">Дата окончания.</param>
public record CreateEventRequest(
	[Required] int Id,
	[Required] string Title,
	string? Description,
	[Required] DateTime StartAt,
	[Required] DateTime EndAt);