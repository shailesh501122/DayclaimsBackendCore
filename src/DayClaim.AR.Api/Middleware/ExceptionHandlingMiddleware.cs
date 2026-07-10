using System.Net;
using DayClaim.AR.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Middleware;

/// <summary>
/// Maps domain/application exceptions to RFC 7807 ProblemDetails responses.
/// Deliberately never leaks stack traces or internal messages for
/// unexpected (500) failures — only the mapped, intentional exception types
/// return their message to the client.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Forbidden"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            ValidationAppException => (HttpStatusCode.BadRequest, "Validation failed"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred"),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogInformation("Request {Method} {Path} failed with {StatusCode}: {Message}",
                context.Request.Method, context.Request.Path, (int)statusCode, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = statusCode == HttpStatusCode.InternalServerError ? "An unexpected error occurred. Please try again." : exception.Message,
            Instance = context.Request.Path,
        };

        if (exception is ValidationAppException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
