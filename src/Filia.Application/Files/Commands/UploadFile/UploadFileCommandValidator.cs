using FluentValidation;

namespace Filia.Application.Files.Commands.UploadFile;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] BlockedExtensions = [".exe", ".dll", ".bat", ".sh", ".cmd"];

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(260)
            .Must(name => !BlockedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage("This file type is not allowed.");

        RuleFor(x => x.ContentType)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotNull();
    }
}
