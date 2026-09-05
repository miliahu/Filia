using MediatR;

namespace Filia.Application.Files.Queries.GetDownloadUrl;

public record GetDownloadUrlQuery(Guid Id) : IRequest<string>;
