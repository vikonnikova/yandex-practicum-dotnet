namespace Events.Api.Contracts;

/// <summary>
/// Представляет результат пагинации.
/// </summary>
/// <param name="Data">Данные.</param>
/// <param name="Meta">Дополнительные данные.</param>
public record PaginatedResult<T>(IReadOnlyCollection<T> Data, Metadata Meta);

/// <summary>
/// Представляет дополнительные данные.
/// </summary>
/// <param name="TotalItems">Общее количество элементов.</param>
/// <param name="CurrentPage">Номер текущей страницы.</param>
/// <param name="ItemsPerPage">Количество элементов на текущей странице.</param>
public record Metadata(int TotalItems, int CurrentPage, int ItemsPerPage);