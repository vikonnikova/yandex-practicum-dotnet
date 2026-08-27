using Events.Infrastructure.HealthChecker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для проверки «здоровья» приложения.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class HealthController(IDatabaseHealthChecker dbHealthChecker) : ControllerBase
{
    /// <summary>
    /// Проверяет доступ до базы данных.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CheckDbConnection()
    {
        var canConnect = dbHealthChecker.Check();
        return canConnect
            ? Ok("Connection is Ok")
            : StatusCode(500, "Cannot connect to database");
    }
}