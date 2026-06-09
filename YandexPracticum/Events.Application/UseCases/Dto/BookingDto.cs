using Events.Domain;

namespace Events.Application.UseCases.Dto;

public record BookingDto(Guid Id, Guid EventId, BookingStatus Status);