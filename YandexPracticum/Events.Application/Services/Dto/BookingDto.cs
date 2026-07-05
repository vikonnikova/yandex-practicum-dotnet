using Events.Domain;

namespace Events.Application.Services.Dto;

public record BookingDto(Guid BookingId, Guid EventId, BookingStatus Status);