using Bookings.Domain;
using MediatR;

namespace Bookings.Application.Contracts.Commands;

public record CreateBookingCommand(Guid EventId) : IRequest<Booking>;