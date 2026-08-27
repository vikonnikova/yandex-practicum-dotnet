using System.Diagnostics;
using Bookings.Application.Exceptions;
using Bookings.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using AuthenticationException = System.Security.Authentication.AuthenticationException;

namespace Bookings.Api.Middleware;

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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        context.Response.ContentType = "application/json";
        var message = exception.Message;
        int statusCode;

        switch (exception)
        {
            case EntityNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                break;

            case ArgumentException:
            case BookingMustBeInPendingStatusException:
                statusCode = StatusCodes.Status400BadRequest;
                break;

            case UserAlreadyExistsException:
                statusCode = StatusCodes.Status409Conflict;
                break;

            case AuthenticationException:
                statusCode = StatusCodes.Status401Unauthorized;
                break;

            case AccessDeniedException:
                statusCode = StatusCodes.Status403Forbidden;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Internal server error";
                logger.LogError(exception, "Ошибка при обработке запроса {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, traceId);
                break;
        }

        context.Response.StatusCode = statusCode;

        var responseMessage = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Detail = message
        };

        return context.Response.WriteAsJsonAsync(responseMessage);
    }
}