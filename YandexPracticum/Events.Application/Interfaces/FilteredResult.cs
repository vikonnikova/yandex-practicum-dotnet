namespace Events.Application.Interfaces;

public record FilteredResult<T>(int TotalItems, IReadOnlyCollection<T> Data) where T : class;