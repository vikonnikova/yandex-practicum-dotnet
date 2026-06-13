using Events.Domain;

namespace Events.Application.UseCases.Dto;

public record BookingDto(Guid BookingId, Guid EventId, BookingStatus Status);