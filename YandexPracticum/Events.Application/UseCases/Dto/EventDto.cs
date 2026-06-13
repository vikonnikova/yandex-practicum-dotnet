namespace Events.Application.UseCases.Dto;

public record EventDto(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats);

public record EventInfoDto(
	Guid Id,
	string Title,
	string? Description,
	DateTime StartAt,
	DateTime EndAt,
	int TotalSeats,
	int AvailableSeats);