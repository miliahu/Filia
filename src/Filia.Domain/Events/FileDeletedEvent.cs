using Filia.Domain.Common;

namespace Filia.Domain.Events;

public sealed class FileDeletedEvent(Guid fileId, string storagePath) : IDomainEvent
{
    public Guid FileId { get; } = fileId;
    public string StoragePath { get; } = storagePath;
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
