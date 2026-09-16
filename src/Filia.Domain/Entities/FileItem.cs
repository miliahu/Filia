using Filia.Domain.Common;
using Filia.Domain.Enums;
using Filia.Domain.Events;
using Filia.Domain.Exceptions;

namespace Filia.Domain.Entities;

/// <summary>
/// Aggregate root representing a single stored file. The binary content itself
/// lives in the object storage backend (RustFS); this entity is the metadata
/// record and the consistency boundary around it.
/// </summary>
public class FileItem : BaseAuditableEntity
{
    private FileItem() { } // EF Core

    public string FileName { get; private set; } = default!;
    public string? FileTitle { get; set; }
    public string? Description { get; set; }
    public string ContentType { get; private set; } = default!;
    public long SizeInBytes { get; private set; }
    public string StorageBucket { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!;
    public string Checksum { get; private set; } = default!;
    public string? FolderPath { get; private set; }
    public FileStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }

    public static FileItem Create(
        string fileName,
        string contentType,
        long sizeInBytes,
        string storageBucket,
        string storagePath,
        string checksum,
        string? folderPath = null,
        string? fileTitle= null,
        string? description= null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name cannot be empty.");

        if (sizeInBytes <= 0)
            throw new DomainException("File size must be greater than zero.");

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new DomainException("Storage path cannot be empty.");

        var file = new FileItem
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            FileTitle =fileTitle,
            Description = description,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            SizeInBytes = sizeInBytes,
            StorageBucket = storageBucket,
            StoragePath = storagePath,
            Checksum = checksum,
            FolderPath = folderPath,
            Status = FileStatus.Stored,
            IsDeleted = false
        };

        file.AddDomainEvent(new FileUploadedEvent(file.Id, file.FileName, file.SizeInBytes, file.StoragePath));
        return file;
    }

    public void Rename(string newFileName)
    {
        if (string.IsNullOrWhiteSpace(newFileName))
            throw new DomainException("File name cannot be empty.");

        FileName = newFileName;
        AddDomainEvent(new FileMetadataUpdatedEvent(Id));
    }

    public void MoveToFolder(string? folderPath)
    {
        FolderPath = folderPath;
        AddDomainEvent(new FileMetadataUpdatedEvent(Id));
    }

    public void MarkDeleted()
    {
        if (IsDeleted)
            throw new DomainException("File is already deleted.");

        IsDeleted = true;
        Status = FileStatus.Deleted;
        AddDomainEvent(new FileDeletedEvent(Id, StoragePath));
    }

    public void Quarantine()
    {
        Status = FileStatus.Quarantined;
    }
}
