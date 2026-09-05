namespace Filia.Api.Contracts;

public class UploadFileRequest
{
    public required IFormFile File { get; init; }
    public string? FolderPath { get; init; }
}
