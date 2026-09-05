using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Commands.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand>
{
    private readonly IFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storageService;

    public DeleteFileCommandHandler(IFileRepository repository, IUnitOfWork unitOfWork, IFileStorageService storageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        entity.MarkDeleted();
        await _storageService.DeleteAsync(entity.StoragePath, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
