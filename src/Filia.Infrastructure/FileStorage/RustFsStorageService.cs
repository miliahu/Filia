using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Filia.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Filia.Infrastructure.FileStorage;

/// <summary>
/// IFileStorageService implementation backed by RustFS (an S3 API compatible
/// object storage server). Because RustFS implements the S3 protocol, the
/// standard AWSSDK.S3 client works against it unmodified - only the endpoint,
/// credentials and path-style-addressing flag differ from AWS.
/// </summary>
public class RustFsStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly RustFsOptions _options;
    private readonly ILogger<RustFsStorageService> _logger;

    public RustFsStorageService(IAmazonS3 s3Client, IOptions<RustFsOptions> options, ILogger<RustFsStorageService> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UploadedObjectInfo> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var objectKey = $"{DateTimeOffset.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}-{Path.GetFileName(fileName)}";

        using var buffered = new MemoryStream();
        await content.CopyToAsync(buffered, cancellationToken);
        buffered.Position = 0;

        var checksum = Convert.ToHexString(await SHA256.HashDataAsync(buffered, cancellationToken));
        buffered.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            InputStream = buffered,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        _logger.LogInformation("Uploaded object {ObjectKey} ({Size} bytes) to RustFS bucket {Bucket}",
            objectKey, buffered.Length, _options.BucketName);

        return new UploadedObjectInfo(_options.BucketName, objectKey, checksum, buffered.Length);
    }

    public async Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var response = await _s3Client.GetObjectAsync(_options.BucketName, storagePath, cancellationToken);

        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        await _s3Client.DeleteObjectAsync(_options.BucketName, storagePath, cancellationToken);
        _logger.LogInformation("Deleted object {ObjectKey} from RustFS bucket {Bucket}", storagePath, _options.BucketName);
    }

    public Task<string> GetPresignedDownloadUrlAsync(string storagePath, TimeSpan expiry, CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storagePath,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        return Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _options.BucketName);
        if (!exists)
        {
            await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _options.BucketName }, cancellationToken);
        }
    }
}
