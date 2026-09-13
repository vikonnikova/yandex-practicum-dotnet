using System.ComponentModel.DataAnnotations;

namespace Events.Api.Contracts;

/// <summary>
/// Представляет данные для пагинации и фильтрации.
/// </summary>
public record GetEventsQuery
{
    /// <summary>
    /// Фильтр по наименованию события.
    /// </summary>
    public string? Title { get; init; }
    /// <summary>
    /// Фильтр по дате начала события.
    /// </summary>
    public DateTime? From { get; init; }
    /// <summary>
    /// Фильтр по дате завершения события.
    /// </summary>
    public DateTime? To { get; init; }

    /// <summary>
    /// Номер текущей страницы.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть больше или равен 1.")]
    public int Page { get; init; } = 1;
    /// <summary>
    /// Количество элементов на странице.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Количество элементов на странице должно быть от 1 до 100.")]
    public int PageSize { get; init; } = 10;
};