using AutoMapper;
using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Queries.GetFileById;

public class GetFileByIdQueryHandler(IFileRepository repository, IMapper mapper) : IRequestHandler<GetFileByIdQuery, FileDetailsDto>
{
    public async Task<FileDetailsDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        return mapper.Map<FileDetailsDto>(entity);
    }
}
