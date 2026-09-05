using Filia.Domain.Common;

namespace Filia.Domain.Events;

public sealed class FileDeletedEvent : IDomainEvent
{
    public FileDeletedEvent(Guid fileId, string storagePath)
    {
        FileId = fileId;
        StoragePath = storagePath;
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid FileId { get; }
    public string StoragePath { get; }
    public DateTimeOffset OccurredOn { get; }
}
