using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.Mappings;

internal static class EventMapping
{
	public static EventDto ToDto(this Event entity)
	{
		return new EventDto(entity.Id, entity.Title, entity.Description, entity.Period.StartAt, entity.Period.EndAt);
	}
}