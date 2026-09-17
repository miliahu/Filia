using Filia.Domain.Common;

namespace Filia.Domain.Events;

public sealed class FileUploadedEvent(Guid fileId, string fileName, long sizeInBytes, string storagePath)
    : IDomainEvent
{
    public Guid FileId { get; } = fileId;
    public string FileName { get; } = fileName;
    public long SizeInBytes { get; } = sizeInBytes;
    public string StoragePath { get; } = storagePath;
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
