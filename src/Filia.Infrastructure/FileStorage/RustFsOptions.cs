namespace Filia.Infrastructure.FileStorage;

/// <summary>
/// RustFS speaks the S3 API, so we talk to it through the standard AWS S3 SDK
/// pointed at RustFS's endpoint with path-style addressing enabled.
/// </summary>
public class RustFsOptions
{
    public const string SectionName = "RustFs";

    public required string Endpoint { get; init; }
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string BucketName { get; init; }
    public string Region { get; init; } = "us-east-1";
    public bool ForcePathStyle { get; init; } = true;
}
