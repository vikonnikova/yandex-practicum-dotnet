using Events.Application.Exceptions;

namespace Events.Api.Middleware;

internal class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		int statusCode;
		string message;

		switch (exception)
		{
			case EntityNotFoundException:
				statusCode = StatusCodes.Status404NotFound;
				message = exception.Message;
				break;

			case ArgumentException:
				statusCode = StatusCodes.Status400BadRequest;
				message = exception.Message;
				break;

			default:
				statusCode = StatusCodes.Status500InternalServerError;
				message = "Internal server error";
				break;
		}

		context.Response.StatusCode = statusCode;

		return context.Response.WriteAsJsonAsync(new CustomHttpResponse(message));
	}
}