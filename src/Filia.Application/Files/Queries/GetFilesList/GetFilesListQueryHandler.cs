using AutoMapper;
using Filia.Application.Common.Interfaces;
using Filia.Application.Common.Models;
using MediatR;

namespace Filia.Application.Files.Queries.GetFilesList;

public class GetFilesListQueryHandler(IFileRepository repository, IMapper mapper) : IRequestHandler<GetFilesListQuery, PaginatedList<FileListItemDto>>
{
    public async Task<PaginatedList<FileListItemDto>> Handle(GetFilesListQuery request, CancellationToken cancellationToken)
    {
        var paged = await repository.GetPagedAsync(
            request.FolderPath,
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = mapper.Map<List<FileListItemDto>>(paged.Items);

        return new PaginatedList<FileListItemDto>(items, paged.TotalCount, request.PageNumber, request.PageSize);
    }
}
