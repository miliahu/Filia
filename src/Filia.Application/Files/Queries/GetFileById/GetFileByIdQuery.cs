using MediatR;

namespace Filia.Application.Files.Queries.GetFileById;

public record GetFileByIdQuery(Guid Id) : IRequest<FileDetailsDto>;
