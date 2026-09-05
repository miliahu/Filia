using Filia.Application.Common.Exceptions;
using Filia.Application.Common.Interfaces;
using Filia.Application.Files.Commands.DeleteFile;
using Filia.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Filia.UnitTests.Application.Files.Commands;

/// <summary>
/// Exercises the handler against mocked IFileRepository / IUnitOfWork / IFileStorageService
/// - no EF Core or database involved, since Application depends only on those
/// plain interfaces (see LayerDependencyTests.Application_ShouldNotHaveDependencyOnEntityFrameworkCore).
/// </summary>
public class DeleteFileCommandHandlerTests
{
    private readonly IFileRepository _repository = Substitute.For<IFileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IFileStorageService _storage = Substitute.For<IFileStorageService>();

    [Fact]
    public async Task Handle_WithExistingFile_ShouldMarkDeletedAndCallStorage()
    {
        var file = FileItem.Create("a.txt", "text/plain", 10, "bucket", "path/a.txt", "checksum");
        _repository.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var handler = new DeleteFileCommandHandler(_repository, _unitOfWork, _storage);

        await handler.Handle(new DeleteFileCommand(file.Id), CancellationToken.None);

        file.IsDeleted.Should().BeTrue();
        await _storage.Received(1).DeleteAsync("path/a.txt", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingFile_ShouldThrowNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FileItem?)null);

        var handler = new DeleteFileCommandHandler(_repository, _unitOfWork, _storage);

        var act = async () => await handler.Handle(new DeleteFileCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
