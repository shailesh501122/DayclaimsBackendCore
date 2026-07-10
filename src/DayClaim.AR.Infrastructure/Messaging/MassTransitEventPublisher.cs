using DayClaim.AR.Application.Common.Interfaces;
using MassTransit;

namespace DayClaim.AR.Infrastructure.Messaging;

/// <summary>
/// Publishes onto RabbitMQ via MassTransit — the local stand-in for
/// AmazonMQ (deck: "AmazonMQ used for async microservice-to-microservice
/// tasks (push) on top of streams"). One exchange per event type; consumers
/// (e.g. a future Rule Engine auto-trigger) bind their own queues.
/// </summary>
public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class =>
        publishEndpoint.Publish(message, cancellationToken);
}
