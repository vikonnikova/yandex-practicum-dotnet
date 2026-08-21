using Events.Api.Contracts;
using Events.Application.Services.Dto;

namespace Events.Api.Mappings;

internal static class BookingMapping
{
	internal static BookingToAddDto ToDto(Guid eventId, BookingRequest data)
	{
		return new BookingToAddDto(eventId, data.UserId);
	}

	internal static BookingResponse ToResponse(this BookingDto dto)
	{
		return new BookingResponse(dto.BookingId, dto.EventId, MapStatus(dto.Status));
	}

	private static BookingStatus MapStatus(Events.Domain.BookingStatus value)
	{
		return value switch
		{
			Domain.BookingStatus.Pending => BookingStatus.Pending,
			Domain.BookingStatus.Confirmed => BookingStatus.Confirmed,
			Domain.BookingStatus.Rejected => BookingStatus.Rejected,
			_ => throw new ArgumentException($"Не найден маппинг для {value}")
		};
	}
}