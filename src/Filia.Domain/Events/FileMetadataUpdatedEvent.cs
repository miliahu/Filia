using Filia.Domain.Common;

namespace Filia.Domain.Events;

public sealed class FileMetadataUpdatedEvent(Guid fileId) : IDomainEvent
{
    public Guid FileId { get; } = fileId;
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
