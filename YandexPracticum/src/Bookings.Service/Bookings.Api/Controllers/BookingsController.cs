using Bookings.Api.Contracts.Bookings;
using Bookings.Api.Mapping;
using Bookings.Application.Contracts.Commands;
using Bookings.Application.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookings.Api.Controllers;

/// <summary>
/// Представляет контроллер для бронирования.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class BookingsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Возвращает информацию о бронировании по идентификатору.
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
    /// Создает заявку на бронирование.
    /// </summary>
    /// <param name="eventId">Идентификатор события.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost("{eventId:guid}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create([FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var result = (await sender.Send(new CreateBookingCommand(eventId), cancellationToken)).ToResponse();

        var statusUrl = Url.Action(nameof(GetById), "Bookings", new { id = result.BookingId });
        Response.Headers.Location = statusUrl;

        return Accepted(result);
    }

    /// <summary>
    /// Отменяет заявку на бронирование.
    /// </summary>
    /// <param name="id">Идентификатор брони.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelBookingCommand(id), cancellationToken);
        return Ok();
    }
}