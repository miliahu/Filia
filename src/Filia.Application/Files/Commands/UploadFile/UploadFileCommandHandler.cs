using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Commands.UploadFile;

public class UploadFileCommandHandler(IFileRepository repository, IUnitOfWork unitOfWork, IFileStorageService storageService) : IRequestHandler<UploadFileCommand, UploadFileResult>
{
    public async Task<UploadFileResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var uploaded = await storageService.UploadAsync(
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

        await repository.AddAsync(fileItem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadFileResult(fileItem.Id, fileItem.FileName, fileItem.SizeInBytes, fileItem.StoragePath);
    }
}
