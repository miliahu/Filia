using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Commands.DeleteFile;

public class DeleteFileCommandHandler(IFileRepository repository, IUnitOfWork unitOfWork, IFileStorageService storageService) : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        entity.MarkDeleted();
        await storageService.DeleteAsync(entity.StoragePath, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
