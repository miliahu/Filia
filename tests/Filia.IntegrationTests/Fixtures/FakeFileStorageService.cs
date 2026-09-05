using System.Security.Cryptography;
using Filia.Application.Common.Interfaces;

namespace Filia.IntegrationTests.Fixtures;

/// <summary>
/// Stands in for RustFS during integration tests: keeps object bytes in
/// memory instead of talking to a real S3-compatible endpoint, so the tests
/// exercise the full API/Application/Persistence stack without external deps.
/// </summary>
public class FakeFileStorageService : IFileStorageService
{
    private static readonly Dictionary<string, byte[]> Store = new();

    public async Task<UploadedObjectInfo> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        var key = $"test/{Guid.NewGuid():N}-{fileName}";
        Store[key] = bytes;

        var checksum = Convert.ToHexString(SHA256.HashData(bytes));
        return new UploadedObjectInfo("test-bucket", key, checksum, bytes.LongLength);
    }

    public Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken)
    {
        Stream stream = new MemoryStream(Store.TryGetValue(storagePath, out var bytes) ? bytes : []);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        Store.Remove(storagePath);
        return Task.CompletedTask;
    }

    public Task<string> GetPresignedDownloadUrlAsync(string storagePath, TimeSpan expiry, CancellationToken cancellationToken) =>
        Task.FromResult($"https://fake-rustfs.local/{storagePath}?expires={expiry.TotalSeconds}");
}
