using MediatR;

namespace Events.Application.Contracts.Commands.Events;

public record UpdateEventCommand(
	Guid Id,
	string Title,
	string? Description,
	DateTime StartAt,
	DateTime EndAt,
	int TotalSeats) : IRequest;