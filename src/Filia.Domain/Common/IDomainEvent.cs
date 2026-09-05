namespace Filia.Domain.Common;

/// <summary>
/// Marker interface for domain events raised by entities.
/// Infrastructure maps these to MediatR notifications for dispatch.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
