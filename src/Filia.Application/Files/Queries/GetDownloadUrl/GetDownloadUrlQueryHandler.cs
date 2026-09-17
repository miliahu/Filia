using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Queries.GetDownloadUrl;

public class GetDownloadUrlQueryHandler(IFileRepository repository, IFileStorageService storageService) : IRequestHandler<GetDownloadUrlQuery, string>
{
    public async Task<string> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        return await storageService.GetPresignedDownloadUrlAsync(
            entity.StoragePath,
            TimeSpan.FromMinutes(15),
            cancellationToken);
    }
}
