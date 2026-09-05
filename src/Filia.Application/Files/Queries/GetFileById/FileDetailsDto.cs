namespace Filia.Application.Files.Queries.GetFileById;

public class FileDetailsDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long SizeInBytes { get; init; }
    public string? FolderPath { get; init; }
    public string Checksum { get; init; } = default!;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
}
