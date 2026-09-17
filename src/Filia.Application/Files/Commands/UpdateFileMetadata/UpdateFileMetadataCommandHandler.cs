using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Commands.UpdateFileMetadata;

public class UpdateFileMetadataCommandHandler(IFileRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateFileMetadataCommand>
{
    public async Task Handle(UpdateFileMetadataCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        entity.Rename(request.FileName);
        entity.MoveToFolder(request.FolderPath);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
