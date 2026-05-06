using System.ComponentModel.DataAnnotations;
using Events.Api.Validation;

namespace Events.Api.Contracts;

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
	[Required(ErrorMessage = "Наименование события обязательно для заполнения.")]
	string Title,
	string? Description,
	[NotDefault(ErrorMessage = "Дата начала события обязательна для заполнения.")]
	DateTime StartAt,
	[NotDefault(ErrorMessage = "Дата окончания события обязательна для заполнения.")]
	DateTime EndAt)
	: EventRequest(Title, Description, StartAt, EndAt);