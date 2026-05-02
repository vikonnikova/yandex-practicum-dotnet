using Events.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
	[HttpGet]
	public IActionResult GetAll()
	{
		return NoContent();
	}

	[HttpGet("{id:int}")]
	public IActionResult GetById(int id)
	{
		return NoContent();
	}

	[HttpPost]
	public IActionResult Create([FromBody] CreateEventDto eventData)
	{
		return NoContent();
	}

	[HttpPut("{id:int}")]
	public IActionResult Update(int id, UpdateEventDto eventData)
	{
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public IActionResult Delete()
	{
		return NoContent();
	}
}