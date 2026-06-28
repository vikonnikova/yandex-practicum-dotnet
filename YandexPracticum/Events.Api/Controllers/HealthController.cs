using Events.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для бронирования.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController(AppDbContext context) : ControllerBase
{
	/// <summary>
	/// Проверяет доступ до базы данных.
	/// </summary>
	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public IActionResult CheckDbConnection()
	{
		var canConnect = context.Database.CanConnect();
		return canConnect
			? Ok("Connected to database")
			: StatusCode(500, "Cannot connect to database");
	}
}