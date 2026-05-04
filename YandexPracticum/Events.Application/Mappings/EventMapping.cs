using Events.Application.Dto;
using Events.Domain;

namespace Events.Application.Mappings;

public static class EventMapping
{
	public static EventResponse ToDto(this Event eventEntity)
	{
		return new EventResponse(eventEntity.Id, eventEntity.Title, eventEntity.Description, eventEntity.Period.StartAt,
			eventEntity.Period.EndAt);
	}
}