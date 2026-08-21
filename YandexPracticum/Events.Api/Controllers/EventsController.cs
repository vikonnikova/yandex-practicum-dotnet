using Events.Api.Contracts;
using Events.Api.Mappings;
using Events.Application;
using Events.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для событий.
/// </summary>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService, IBookingService bookingService)
	: ControllerBase
{
	/// <summary>
	/// Возвращает все события.
	/// </summary>
	/// <param name="query">Фильтры и пагинация.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpGet]
	[ProducesResponseType(typeof(PaginatedResult<EventResponse>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginatedResult<EventResponse>>> GetAll([FromQuery] GetEventsQuery query,
		CancellationToken cancellationToken)
	{
		var result = await eventService.GetBy(query.Page, query.PageSize,
			new Filters(query.Title, query.From, query.To), cancellationToken);

		return Ok(result.ToPaginatedResponse());
	}

	/// <summary>
	/// Возвращает событие по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<EventResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		return Ok(await eventService.GetById(id, cancellationToken));
	}

	/// <summary>
	/// Создает событие.
	/// </summary>
	/// <param name="eventRequest">Данные для создания.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpPost]
	[ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<EventResponse>> Create([FromBody] EventRequest eventRequest,
		CancellationToken cancellationToken)
	{
		var result = await eventService.Add(eventRequest.ToDto(), cancellationToken);

		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	/// <summary>
	/// Обновляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	/// <param name="eventRequest">Данные для обновления.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EventRequest eventRequest,
		CancellationToken cancellationToken)
	{
		await eventService.Update(eventRequest.ToDto(id), cancellationToken);
		return NoContent();
	}

	/// <summary>
	/// Удаляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		await eventService.Remove(id, cancellationToken);
		return Ok();
	}

	/// <summary>
	/// Создает заявку на бронирование.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	/// <param name="data">Данные для бронирования.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpPost("{id:guid}/book")]
	[ProducesResponseType(typeof(BookingResponse), StatusCodes.Status202Accepted)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<ActionResult<BookingResponse>> Book([FromRoute] Guid id, [FromBody] BookingRequest data,
		CancellationToken cancellationToken)
	{
		var result = await bookingService.Add(BookingMapping.ToDto(id, data), cancellationToken);

		var statusUrl = Url.Action(nameof(BookingsController.GetById), "Bookings", new { id = result.BookingId });
		Response.Headers.Location = statusUrl;

		return Accepted(result);
	}
}