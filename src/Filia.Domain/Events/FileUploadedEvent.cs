using Filia.Domain.Common;

namespace Filia.Domain.Events;

public sealed class FileUploadedEvent : IDomainEvent
{
    public FileUploadedEvent(Guid fileId, string fileName, long sizeInBytes, string storagePath)
    {
        FileId = fileId;
        FileName = fileName;
        SizeInBytes = sizeInBytes;
        StoragePath = storagePath;
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid FileId { get; }
    public string FileName { get; }
    public long SizeInBytes { get; }
    public string StoragePath { get; }
    public DateTimeOffset OccurredOn { get; }
}
