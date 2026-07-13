namespace Events.Application.Services.Dto;

public record PaginatedResult<T>(int TotalItems, int CurrentPage, int ItemsPerPage, IReadOnlyList<T> Items);