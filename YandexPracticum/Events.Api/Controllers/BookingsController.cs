using Events.Api.Contracts;
using Events.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для бронирования.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
	/// <summary>
	/// Возвращает бронь по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор брони.</param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public ActionResult<BookingResponse> GetById([FromRoute] Guid id)
	{
		return Ok(bookingService.GetById(id));
	}
}