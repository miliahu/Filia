namespace Filia.Application.Common.Interfaces;

/// <summary>
/// Publishes integration events to the message broker (RabbitMQ).
/// Distinct from MediatR notifications, which stay in-process for domain events.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) where TEvent : class;
}
