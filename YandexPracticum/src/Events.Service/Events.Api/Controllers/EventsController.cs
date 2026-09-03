using Events.Api.Contracts.Events;
using Events.Api.Mappings;
using Events.Application.Contracts.Commands;
using Events.Application.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для событий.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class EventsController(ISender sender)
    : ControllerBase
{
    /// <summary>
    /// Возвращает все события.
    /// </summary>
    /// <param name="data">Фильтры и пагинация.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<EventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<EventResponse>>> GetAll([FromQuery] GetEventsQuery data,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(data.ToQuery(), cancellationToken);

        return Ok(result.ToResponse());
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
        return Ok((await sender.Send(new GetEventByIdQuery(id), cancellationToken)).ToResponse());
    }

    /// <summary>
    /// Создает событие.
    /// </summary>
    /// <param name="data">Данные для создания.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Create([FromBody] EventRequest data,
        CancellationToken cancellationToken)
    {
        var eventId = await sender.Send(data.ToAddCommand(), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = eventId }, eventId);
    }

    /// <summary>
    /// Обновляет событие.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="data">Данные для обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EventRequest data,
        CancellationToken cancellationToken)
    {
        await sender.Send(data.ToUpdateCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удаляет событие.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteEventCommand(id), cancellationToken);
        return Ok();
    }
}