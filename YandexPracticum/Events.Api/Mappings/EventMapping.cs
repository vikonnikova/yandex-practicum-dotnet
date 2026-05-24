using Events.Api.Contracts;
using Events.Application.UseCases.Dto;

namespace Events.Api.Mappings;

internal static class EventMapping
{
	internal static EventDto ToDto(this EventRequest eventData, int eventId)
	{
		return new EventDto(eventId, eventData.Title, eventData.Description, eventData.StartAt, eventData.EndAt);
	}

	internal static Contracts.PaginatedResult<EventResponse> ToPaginatedResponse(
		this Application.UseCases.Dto.PaginatedResult<EventDto> paginatedEvents)
	{
		return new Contracts.PaginatedResult<EventResponse>(paginatedEvents.ToResponse(), paginatedEvents.ToMetadata());
	}

	private static IReadOnlyCollection<EventResponse> ToResponse(
		this Application.UseCases.Dto.PaginatedResult<EventDto> paginatedEvents)
	{
		return paginatedEvents.Items.Select(x => new EventResponse(x.Id, x.Title, x.Description, x.StartAt, x.EndAt))
			.ToArray();
	}

	private static Metadata ToMetadata(this Application.UseCases.Dto.PaginatedResult<EventDto> paginatedEvents)
	{
		return new Metadata(paginatedEvents.TotalItems, paginatedEvents.CurrentPage, paginatedEvents.ItemsPerPage);
	}
}