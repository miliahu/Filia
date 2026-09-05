using AutoMapper;
using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using MediatR;

namespace Filia.Application.Files.Queries.GetFileById;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, FileDetailsDto>
{
    private readonly IFileRepository _repository;
    private readonly IMapper _mapper;

    public GetFileByIdQueryHandler(IFileRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<FileDetailsDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new NotFoundException(nameof(FileItem), request.Id);

        return _mapper.Map<FileDetailsDto>(entity);
    }
}
