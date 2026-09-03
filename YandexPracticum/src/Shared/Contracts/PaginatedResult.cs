namespace Shared.Contracts;

/// <summary>
/// Представляет результат пагинации.
/// </summary>
/// <param name="Data">Данные.</param>
/// <param name="TotalItems">Общее количество элементов.</param>
public record PaginatedResult<T>(IReadOnlyCollection<T> Data, int TotalItems) where T : class;