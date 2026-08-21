using MediatR;

namespace Events.Application.Contracts.Commands.Events;

public record AddEventCommand(
	string Title,
	string? Description,
	DateTime StartAt,
	DateTime EndAt,
	int TotalSeats) : IRequest<Guid>;