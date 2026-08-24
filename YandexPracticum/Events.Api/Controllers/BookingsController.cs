using Events.Api.Contracts.Bookings;
using Events.Api.Mappings;
using Events.Application.Contracts.Commands.Bookings;
using Events.Application.Contracts.Queries.Bookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для бронирования.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class BookingsController(ISender sender) : ControllerBase
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
		return Ok((await sender.Send(new GetBookingByIdQuery(id), cancellationToken)).ToResponse());
	}
	
	/// <summary>
	/// Удаляет бронь.
	/// </summary>
	/// <param name="id">Идентификатор брони.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<BookingResponse>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		await sender.Send(new RemoveBookingCommand(id), cancellationToken);
		return Ok();
	}
}