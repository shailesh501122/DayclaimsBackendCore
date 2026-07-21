using DayClaim.AR.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace DayClaim.AR.Infrastructure.Common;

/// <summary>
/// In-process stand-in for a message broker. There is currently no consumer
/// anywhere in this solution — the only publish site (IngestFileCommand) has
/// nothing subscribing to ClaimImportedEvent yet — so running RabbitMQ for
/// this alone was pure resource cost on a single-box deployment. If a real
/// consumer shows up (e.g. an async Rule Engine trigger), swap this back for
/// a broker-backed IEventPublisher rather than reaching for one preemptively.
/// </summary>
public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        logger.LogInformation("Event published: {EventType} {@Event}", typeof(T).Name, message);
        return Task.CompletedTask;
    }
}
