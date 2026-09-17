using Filia.Domain.Common;
using MediatR;

namespace Filia.Application.Common.Models;

/// <summary>
/// Wraps a domain event so it can travel through the MediatR pipeline as a notification.
/// </summary>
public class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
