using Filia.Application.Common.Models;
using MediatR;

namespace Filia.Application.Files.Queries.GetFilesList;

public record GetFilesListQuery : IRequest<PaginatedList<FileListItemDto>>
{
    public string? FolderPath { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
