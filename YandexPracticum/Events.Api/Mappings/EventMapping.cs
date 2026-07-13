using Events.Api.Contracts;
using Events.Application.Services.Dto;

namespace Events.Api.Mappings;

internal static class EventMapping
{
	internal static EventDto ToDto(this EventRequest eventData)
	{
		return new EventDto(eventData.Title, eventData.Description, eventData.StartAt, eventData.EndAt,
			eventData.TotalSeats);
	}
	
	internal static EventToUpdateDto ToDto(this EventRequest eventData, Guid eventId)
	{
		return new EventToUpdateDto(eventId, eventData.Title, eventData.Description,
			eventData.StartAt, eventData.EndAt, eventData.TotalSeats);
	}

	internal static Contracts.PaginatedResult<EventResponse> ToPaginatedResponse(
		this Application.Services.Dto.PaginatedResult<EventInfoDto> paginatedEvents)
	{
		return new Contracts.PaginatedResult<EventResponse>(paginatedEvents.ToResponse(), paginatedEvents.ToMetadata());
	}

	private static IReadOnlyCollection<EventResponse> ToResponse(
		this Application.Services.Dto.PaginatedResult<EventInfoDto> paginatedEvents)
	{
		return paginatedEvents.Items.Select(x =>
				new EventResponse(x.Id, x.Title, x.Description, x.StartAt, x.EndAt, x.TotalSeats, x.AvailableSeats))
			.ToArray();
	}

	private static Metadata ToMetadata(this Application.Services.Dto.PaginatedResult<EventInfoDto> paginatedEvents)
	{
		return new Metadata(paginatedEvents.TotalItems, paginatedEvents.CurrentPage, paginatedEvents.ItemsPerPage);
	}
}