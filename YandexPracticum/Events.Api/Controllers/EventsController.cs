using Events.Application.Dto;
using Events.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
	[HttpGet]
	public ActionResult<IReadOnlyCollection<EventResponse>> GetAll()
	{
		return Ok(eventService.GetAll());
	}

	[HttpGet("{id:int}")]
	public ActionResult<EventResponse?> GetById([FromRoute] int id)
	{
		return Ok(eventService.GetById(id));
	}

	[HttpPost]
	public IActionResult Create([FromBody] CreateEventRequest eventRequest)
	{
		eventService.Add(eventRequest);
		return Created();
	}

	[HttpPut("{id:int}")]
	public IActionResult Update([FromRoute] int id, [FromBody] UpdateEventRequest eventRequest)
	{
		eventService.Update(id, eventRequest);
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public IActionResult Delete([FromRoute] int id)
	{
		eventService.Remove(id);
		return Ok();
	}
}