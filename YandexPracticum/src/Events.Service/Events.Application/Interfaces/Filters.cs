namespace Events.Application;

public record Filters(string? Title = null, DateTime? From = null, DateTime? To = null);