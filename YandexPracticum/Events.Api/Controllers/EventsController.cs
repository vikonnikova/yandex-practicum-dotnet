using Events.Api.Contracts;
using Events.Api.Mappings;
using Events.Api.Middleware;
using Events.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для событий.
/// </summary>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
	/// <summary>
	/// Возвращает все события.
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(IReadOnlyCollection<EventResponse>), StatusCodes.Status200OK)]
	public ActionResult<IReadOnlyCollection<EventResponse>> GetAll([FromQuery] GetEventsQuery query)
	{
		return Ok(eventService.GetBy(new Filters(query.Title, query.From, query.To), query.Page, query.PageSize));
	}

	/// <summary>
	/// Возвращает событие по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	[HttpGet("{id:int}")]
	[ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public ActionResult<EventResponse> GetById([FromRoute] int id)
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
	public IActionResult Create([FromBody] CreateEventRequest eventRequest)
	{
		var result = eventService.Add(eventRequest.ToDto(eventRequest.Id));
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	/// <summary>
	/// Обновляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	/// <param name="eventRequest">Данные для обновления.</param>
	[HttpPut("{id:int}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult Update([FromRoute] int id, [FromBody] EventRequest eventRequest)
	{
		eventService.Update(eventRequest.ToDto(id));
		return NoContent();
	}

	/// <summary>
	/// Удаляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	[HttpDelete("{id:int}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public IActionResult Delete([FromRoute] int id)
	{
		eventService.Remove(id);
		return Ok();
	}
}