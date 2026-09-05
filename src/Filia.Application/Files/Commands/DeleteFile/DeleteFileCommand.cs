using MediatR;

namespace Filia.Application.Files.Commands.DeleteFile;

public record DeleteFileCommand(Guid Id) : IRequest;
