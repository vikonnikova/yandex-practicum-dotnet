using Events.Application.UseCases.Dto;

namespace Events.Api.Mappings;

internal static class BookingMapping
{
	internal static BookingToAddDto ToDto(Guid bookingId, Guid eventId)
	{
		return new BookingToAddDto(bookingId, eventId);
	}
}