using Events.Api.Contracts;
using Events.Api.Mappings;
using Events.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для событий.
/// </summary>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService, IBookingService bookingService) : ControllerBase
{
	/// <summary>
	/// Возвращает все события.
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(PaginatedResult<EventResponse>), StatusCodes.Status200OK)]
	public ActionResult<PaginatedResult<EventResponse>> GetAll([FromQuery] GetEventsQuery query)
	{
		var result = eventService.GetBy(new Filters(query.Title, query.From, query.To), query.Page, query.PageSize);
		return Ok(result.ToPaginatedResponse());
	}

	/// <summary>
	/// Возвращает событие по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public ActionResult<EventResponse> GetById([FromRoute] Guid id)
	{
		return Ok(eventService.GetById(id));
	}

	/// <summary>
	/// Создает событие.
	/// </summary>
	/// <param name="eventRequest">Данные для создания.</param>
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult Create([FromBody] EventRequest eventRequest)
	{
		var eventId = Guid.NewGuid();
		var result = eventService.Add(eventRequest.ToDto(eventId));
		
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	/// <summary>
	/// Обновляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	/// <param name="eventRequest">Данные для обновления.</param>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult Update([FromRoute] Guid id, [FromBody] EventRequest eventRequest)
	{
		eventService.Update(eventRequest.ToDto(id));
		return NoContent();
	}

	/// <summary>
	/// Удаляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public IActionResult Delete([FromRoute] Guid id)
	{
		eventService.Remove(id);
		return Ok();
	}
	
	/// <summary>
	/// Бронирует событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	[HttpPost("{id:guid}/book")]
	[ProducesResponseType(StatusCodes.Status202Accepted)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public IActionResult Book([FromRoute] Guid id)
	{
		var bookingId = Guid.NewGuid();
		var result = bookingService.Add(BookingMapping.ToDto(bookingId, id));
		
		var statusUrl = Url.Action(nameof(BookingController.GetById), new { id = bookingId });
		Response.Headers.Location = statusUrl;
		
		return Accepted(result);
		
		// TODO реализовать возврат 404 при отсутствии события по идентификатору
	}
}