namespace Auth.Application.Contracts;

public record PaginatedResult<T>(int TotalItems, int CurrentPage, int ItemsPerPage, IReadOnlyList<T> Items);