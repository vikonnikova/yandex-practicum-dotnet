using Events.Api.Contracts;
using Events.Api.Mappings;
using Events.Application.Services;
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
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<BookingResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		return Ok((await bookingService.GetById(id, cancellationToken)).ToResponse());
	}
}