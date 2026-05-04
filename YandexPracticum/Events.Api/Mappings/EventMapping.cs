using Events.Api.Contracts;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Api.Mappings;

internal static class EventMapping
{
	public static EventDto ToDto(this CreateEventRequest eventData)
	{
		return new EventDto(eventData.Id, eventData.Title, eventData.Description, eventData.StartAt,
			eventData.EndAt);
	}

	public static EventDto ToDto(this UpdateEventRequest eventData, int eventId)
	{
		return new EventDto(eventId, eventData.Title, eventData.Description, eventData.StartAt, eventData.EndAt);
	}

	public static EventResponse ToDto(this Event eventEntity)
	{
		return new EventResponse(eventEntity.Id, eventEntity.Title, eventEntity.Description, eventEntity.Period.StartAt,
			eventEntity.Period.EndAt);
	}
}