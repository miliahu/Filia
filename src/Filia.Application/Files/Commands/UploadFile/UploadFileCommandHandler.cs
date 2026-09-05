using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Commands.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, UploadFileResult>
{
    private readonly IFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storageService;

    public UploadFileCommandHandler(IFileRepository repository, IUnitOfWork unitOfWork, IFileStorageService storageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task<UploadFileResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var uploaded = await _storageService.UploadAsync(
            request.Content,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var fileItem = FileItem.Create(
            request.FileName,
            request.ContentType,
            uploaded.SizeInBytes,
            uploaded.Bucket,
            uploaded.StoragePath,
            uploaded.Checksum,
            request.FolderPath);

        await _repository.AddAsync(fileItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadFileResult(fileItem.Id, fileItem.FileName, fileItem.SizeInBytes, fileItem.StoragePath);
    }
}
