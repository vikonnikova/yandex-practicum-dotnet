namespace Events.Application.Services.Dto;

public record EventDto(string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats);

public record EventToUpdateDto(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats);

public record EventInfoDto(
	Guid Id,
	string Title,
	string? Description,
	DateTime StartAt,
	DateTime EndAt,
	int TotalSeats,
	int AvailableSeats);