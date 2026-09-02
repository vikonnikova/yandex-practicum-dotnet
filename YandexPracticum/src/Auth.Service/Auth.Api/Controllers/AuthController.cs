using Auth.Api.Contracts.Auth;
using Auth.Api.Mapping;
using Auth.Application.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

/// <summary>
/// Представляет контроллер аутентификации.
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Регистрирует пользователя в системе.
    /// </summary>
    /// <param name="data">Данные для регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Register([FromBody] RegistrationRequest data, CancellationToken cancellationToken)
    {
        await sender.Send(data.ToCommand(), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Осуществляет вход пользователя в систему.
    /// </summary>
    /// <param name="data">Данные для входа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest data,
        CancellationToken cancellationToken)
    {
        var token = await sender.Send(new LoginCommand(data.Login, data.Password), cancellationToken);

        return Ok(token);
    }
}