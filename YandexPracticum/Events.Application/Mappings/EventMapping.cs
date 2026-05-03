using Events.Application.Dto;
using Events.Domain;

namespace Events.Application.Mappings;

public static class EventMapping
{
	public static EventData ToDto(this Event eventEntity)
	{
		return new EventData(eventEntity.Id, eventEntity.Title, eventEntity.Description, eventEntity.StartAt,
			eventEntity.EndAt);
	}
}