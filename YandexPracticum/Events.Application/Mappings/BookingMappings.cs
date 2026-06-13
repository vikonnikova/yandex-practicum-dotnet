using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.Mappings;

internal static class BookingMappings
{
	public static BookingDto ToDto(this Booking entity)
	{
		return new BookingDto(entity.Id, entity.EventId, entity.Status);
	}
}