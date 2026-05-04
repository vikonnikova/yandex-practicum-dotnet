using Events.Api.Contracts;
using Events.Api.Mappings;
using Events.Api.Middleware;
using Events.Api.Validation;
using Events.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для событий.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
	/// <summary>
	/// Возвращает все события.
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(IReadOnlyCollection<EventResponse>), StatusCodes.Status200OK)]
	public ActionResult<IReadOnlyCollection<EventResponse>> GetAll()
	{
		return Ok(eventService.GetAll());
	}

	/// <summary>
	/// Возвращает событие по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	[HttpGet("{id:int}")]
	[ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
	public ActionResult<EventResponse?> GetById([FromRoute] int id)
	{
		return Ok(eventService.GetById(id));
	}

	/// <summary>
	/// Создает событие.
	/// </summary>
	/// <param name="eventRequest">Данные для создания.</param>
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(CustomHttpResponse), StatusCodes.Status400BadRequest)]
	public IActionResult Create([FromBody] CreateEventRequest eventRequest)
	{
		ModelValidator.Validate(eventRequest);
		eventService.Add(eventRequest.ToDto());
		
		return Created();
	}

	/// <summary>
	/// Обновляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	/// <param name="eventRequest">Данные для обновления.</param>
	[HttpPut("{id:int}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(CustomHttpResponse), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(CustomHttpResponse), StatusCodes.Status400BadRequest)]
	public IActionResult Update([FromRoute] int id, [FromBody] UpdateEventRequest eventRequest)
	{
		ModelValidator.Validate(eventRequest);
		eventService.Update(eventRequest.ToDto(id));
		
		return NoContent();
	}

	/// <summary>
	/// Удаляет событие.
	/// </summary>
	/// <param name="id">Идентификатор события.</param>
	[HttpDelete("{id:int}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(CustomHttpResponse), StatusCodes.Status404NotFound)]
	public IActionResult Delete([FromRoute] int id)
	{
		eventService.Remove(id);
		return Ok();
	}
}