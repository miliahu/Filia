namespace Filia.Application.Common.Interfaces;

public record UploadedObjectInfo(string Bucket, string StoragePath, string Checksum, long SizeInBytes);

/// <summary>
/// Abstraction over the object storage backend (RustFS, S3-compatible).
/// Keeping this in Application means the storage technology can be swapped
/// (RustFS today, MinIO/S3 tomorrow) without touching business logic.
/// </summary>
public interface IFileStorageService
{
    Task<UploadedObjectInfo> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);

    Task<string> GetPresignedDownloadUrlAsync(
        string storagePath,
        TimeSpan expiry,
        CancellationToken cancellationToken);
}
