using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Queries.GetDownloadUrl;

public class GetDownloadUrlQueryHandler : IRequestHandler<GetDownloadUrlQuery, string>
{
    private readonly IFileRepository _repository;
    private readonly IFileStorageService _storageService;

    public GetDownloadUrlQueryHandler(IFileRepository repository, IFileStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
    }

    public async Task<string> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        return await _storageService.GetPresignedDownloadUrlAsync(
            entity.StoragePath,
            TimeSpan.FromMinutes(15),
            cancellationToken);
    }
}
