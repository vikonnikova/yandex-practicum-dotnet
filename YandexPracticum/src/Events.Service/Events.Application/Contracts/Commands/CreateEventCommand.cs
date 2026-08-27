using MediatR;

namespace Events.Application.Contracts.Commands;

public record CreateEventCommand(
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt,
    int TotalSeats) : IRequest<Guid>;