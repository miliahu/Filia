using MediatR;

namespace Filia.Application.Files.Commands.UpdateFileMetadata;

public record UpdateFileMetadataCommand : IRequest
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public string? FolderPath { get; init; }
}
