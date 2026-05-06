using Events.Api.Contracts;
using Events.Application.UseCases.Dto;

namespace Events.Api.Mappings;

internal static class EventMapping
{
	public static EventDto ToDto(this EventRequest eventData, int eventId)
	{
		return new EventDto(eventId, eventData.Title, eventData.Description, eventData.StartAt, eventData.EndAt);
	}
}