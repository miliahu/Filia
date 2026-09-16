using MediatR;

namespace Filia.Application.Files.Commands.UploadFile;

public record UploadFileCommand : IRequest<UploadFileResult>
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required Stream Content { get; init; }
    public string? FolderPath { get; init; }
    
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public record UploadFileResult(Guid Id, string FileName, long SizeInBytes, string StoragePath);
