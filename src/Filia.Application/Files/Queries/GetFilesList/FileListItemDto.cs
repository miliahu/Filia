namespace Filia.Application.Files.Queries.GetFilesList;

public class FileListItemDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long SizeInBytes { get; init; }
    public string? FolderPath { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
