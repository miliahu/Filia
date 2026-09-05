using AutoMapper;
using Filia.Application.Common.Interfaces;
using Filia.Application.Common.Models;
using MediatR;

namespace Filia.Application.Files.Queries.GetFilesList;

public class GetFilesListQueryHandler : IRequestHandler<GetFilesListQuery, PaginatedList<FileListItemDto>>
{
    private readonly IFileRepository _repository;
    private readonly IMapper _mapper;

    public GetFilesListQueryHandler(IFileRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FileListItemDto>> Handle(GetFilesListQuery request, CancellationToken cancellationToken)
    {
        var paged = await _repository.GetPagedAsync(
            request.FolderPath,
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = _mapper.Map<List<FileListItemDto>>(paged.Items);

        return new PaginatedList<FileListItemDto>(items, paged.TotalCount, request.PageNumber, request.PageSize);
    }
}
