using System.Diagnostics;
using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DayClaim.AR.Application.Common.Behaviors;

/// <summary>
/// Structured request/response logging for every command and query — the
/// audit trail the deck calls for (CloudWatch-centralized logging target).
/// Never logs request payloads verbatim (PHI risk); only the type name,
/// caller, elapsed time and outcome.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            logger.LogInformation(
                "Handled {RequestName} for user {UserId} in {ElapsedMs}ms",
                requestName, currentUser.UserId, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex, "Request {RequestName} for user {UserId} failed after {ElapsedMs}ms",
                requestName, currentUser.UserId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
