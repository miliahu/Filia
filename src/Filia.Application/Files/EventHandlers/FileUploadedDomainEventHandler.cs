using Filia.Application.Common.Interfaces;
using Filia.Application.Common.Models;
using Filia.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Filia.Application.Files.EventHandlers;

/// <summary>
/// Bridges the in-process domain event to a RabbitMQ integration event so
/// other bounded contexts can react to a file being uploaded. MediatR
/// dispatches this after SaveChangesAsync via the DispatchDomainEventsInterceptor.
/// </summary>
public record FileUploadedIntegrationEvent(Guid FileId, string FileName, long SizeInBytes, string StoragePath);

public class FileUploadedDomainEventHandler : INotificationHandler<DomainEventNotification<FileUploadedEvent>>
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<FileUploadedDomainEventHandler> _logger;

    public FileUploadedDomainEventHandler(IEventBus eventBus, ILogger<FileUploadedDomainEventHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<FileUploadedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("File {FileId} uploaded, publishing integration event", domainEvent.FileId);

        await _eventBus.PublishAsync(
            new FileUploadedIntegrationEvent(domainEvent.FileId, domainEvent.FileName, domainEvent.SizeInBytes, domainEvent.StoragePath),
            cancellationToken);
    }
}
