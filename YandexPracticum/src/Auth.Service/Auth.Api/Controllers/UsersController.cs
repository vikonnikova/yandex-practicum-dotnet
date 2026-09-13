using Auth.Api.Contracts.Auth;
using Auth.Api.Contracts.Users;
using Auth.Api.Mapping;
using Auth.Application.Contracts.Auth;
using Auth.Application.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

/// <summary>
/// Представляет контроллер для пользователей.
/// </summary>
[Authorize]
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
    /// Меняет пароль текущего пользователя.
    /// </summary>
    /// <param name="data">Данные для смены пароля.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest data,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ChangePasswordCommand(data.CurrentPassword, data.NewPassword), cancellationToken);
        return NoContent();
    }
}