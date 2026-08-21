using Events.Api.Contracts.Users;
using Events.Api.Mappings;
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
		await sender.Send(data.ToCommand(), cancellationToken);

		return Ok();
	}
}