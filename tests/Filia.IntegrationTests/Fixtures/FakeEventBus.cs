using Filia.Application.Common.Interfaces;

namespace Filia.IntegrationTests.Fixtures;

public class FakeEventBus : IEventBus
{
    public List<object> PublishedEvents { get; } = new();

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) where TEvent : class
    {
        PublishedEvents.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
