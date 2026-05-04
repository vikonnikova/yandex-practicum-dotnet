using System.ComponentModel.DataAnnotations;

namespace Events.Api.Contracts;

/// <summary>
/// Представляет данные для изменения события.
/// </summary>
/// <param name="Title">Наименование.</param>
/// <param name="Description">Описание.</param>
/// <param name="StartAt">Дата начала.</param>
/// <param name="EndAt">Дата окончания.</param>
public record UpdateEventRequest(
	[Required(ErrorMessage = "Наименование события обязательно для заполнения.")]
	string Title,
	string? Description,
	[Required(ErrorMessage = "Дата начала события обязательна для заполнения.")]
	DateTime StartAt,
	[Required(ErrorMessage = "Дата окончания события обязательна для заполнения.")]
	DateTime EndAt);