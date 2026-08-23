using Events.Api.Contracts.Users;
using Events.Api.Mappings;
using Events.Application.Contracts.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для пользователей.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UsersController(ISender sender) : ControllerBase
{
	/// <summary>
	/// Возвращает пользователя по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор пользователя.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<UserResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		return Ok((await sender.Send(new GetUserByIdQuery(id), cancellationToken)).ToResponse());
	}
	
	/// <summary>
	/// Создает пользователя.
	/// </summary>
	/// <param name="data">Данные для создания.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpPost]
	[ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<UserResponse>> Create([FromBody] UserRequest data,
		CancellationToken cancellationToken)
	{
		var userId = await sender.Send(data.ToCommand(), cancellationToken);

		return CreatedAtAction(nameof(GetById), new { id = userId }, userId);
	}
}