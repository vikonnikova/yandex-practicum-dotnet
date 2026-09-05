using System.ComponentModel.DataAnnotations;

namespace Bookings.Api.Contracts;

/// <summary>
/// Представляет данные для пагинации списка бронирований.
/// </summary>
public record GetBookingsQuery
{
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
}
