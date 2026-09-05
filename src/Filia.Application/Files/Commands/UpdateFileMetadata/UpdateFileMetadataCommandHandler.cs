using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Commands.UpdateFileMetadata;

public class UpdateFileMetadataCommandHandler : IRequestHandler<UpdateFileMetadataCommand>
{
    private readonly IFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFileMetadataCommandHandler(IFileRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateFileMetadataCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        entity.Rename(request.FileName);
        entity.MoveToFolder(request.FolderPath);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
