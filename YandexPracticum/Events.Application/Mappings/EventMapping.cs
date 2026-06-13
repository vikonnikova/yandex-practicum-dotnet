using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.Mappings;

internal static class EventMapping
{
	public static EventInfoDto ToDto(this Event entity)
	{
		return new EventInfoDto(entity.Id, entity.Title, entity.Description,
			entity.Period.StartAt, entity.Period.EndAt, entity.TotalSeats, entity.TotalSeats);
	}
}