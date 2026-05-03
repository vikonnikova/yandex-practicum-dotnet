using Events.Application.Dto;
using Events.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
	[HttpGet]
	public ActionResult<IReadOnlyCollection<EventData>> GetAll()
	{
		return Ok(eventService.GetAll());
	}

	[HttpGet("{id:int}")]
	public ActionResult<EventData?> GetById([FromRoute] int id)
	{
		return Ok(eventService.GetById(id));
	}

	[HttpPost]
	public IActionResult Create([FromBody] CreateEventData eventData)
	{
		eventService.Add(eventData);
		return NoContent();
	}

	[HttpPut("{id:int}")]
	public IActionResult Update([FromRoute] int id, [FromBody] UpdateEventData eventData)
	{
		eventService.Update(id, eventData);
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public IActionResult Delete([FromRoute] int id)
	{
		eventService.Remove(id);
		return NoContent();
	}
}