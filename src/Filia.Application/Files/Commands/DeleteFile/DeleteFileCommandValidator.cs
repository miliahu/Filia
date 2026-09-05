using FluentValidation;

namespace Filia.Application.Files.Commands.DeleteFile;

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
