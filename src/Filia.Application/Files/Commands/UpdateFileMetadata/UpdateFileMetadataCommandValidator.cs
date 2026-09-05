using FluentValidation;

namespace Filia.Application.Files.Commands.UpdateFileMetadata;

public class UpdateFileMetadataCommandValidator : AbstractValidator<UpdateFileMetadataCommand>
{
    public UpdateFileMetadataCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
    }
}
