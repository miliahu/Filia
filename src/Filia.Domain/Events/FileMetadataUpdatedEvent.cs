using Filia.Domain.Common;

namespace Filia.Domain.Events;

public sealed class FileMetadataUpdatedEvent : IDomainEvent
{
    public FileMetadataUpdatedEvent(Guid fileId)
    {
        FileId = fileId;
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid FileId { get; }
    public DateTimeOffset OccurredOn { get; }
}
