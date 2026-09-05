using Filia.Domain.Entities;
using Filia.Domain.Enums;
using Filia.Domain.Events;
using Filia.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Filia.UnitTests.Domain;

public class FileItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceedAndRaiseUploadedEvent()
    {
        var file = FileItem.Create(
            fileName: "invoice.pdf",
            contentType: "application/pdf",
            sizeInBytes: 1024,
            storageBucket: "filia-files",
            storagePath: "2026/09/04/abc-invoice.pdf",
            checksum: "deadbeef");

        file.FileName.Should().Be("invoice.pdf");
        file.Status.Should().Be(FileStatus.Stored);
        file.IsDeleted.Should().BeFalse();
        file.DomainEvents.Should().ContainSingle(e => e is FileUploadedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyFileName_ShouldThrow(string invalidName)
    {
        var act = () => FileItem.Create(invalidName, "text/plain", 10, "bucket", "path", "checksum");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNonPositiveSize_ShouldThrow()
    {
        var act = () => FileItem.Create("a.txt", "text/plain", 0, "bucket", "path", "checksum");

        act.Should().Throw<DomainException>().WithMessage("*greater than zero*");
    }

    [Fact]
    public void MarkDeleted_ShouldSetIsDeletedAndRaiseEvent()
    {
        var file = FileItem.Create("a.txt", "text/plain", 10, "bucket", "path", "checksum");
        file.ClearDomainEvents();

        file.MarkDeleted();

        file.IsDeleted.Should().BeTrue();
        file.Status.Should().Be(FileStatus.Deleted);
        file.DomainEvents.Should().ContainSingle(e => e is FileDeletedEvent);
    }

    [Fact]
    public void MarkDeleted_WhenAlreadyDeleted_ShouldThrow()
    {
        var file = FileItem.Create("a.txt", "text/plain", 10, "bucket", "path", "checksum");
        file.MarkDeleted();

        var act = () => file.MarkDeleted();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_ShouldUpdateFileNameAndRaiseEvent()
    {
        var file = FileItem.Create("old.txt", "text/plain", 10, "bucket", "path", "checksum");
        file.ClearDomainEvents();

        file.Rename("new.txt");

        file.FileName.Should().Be("new.txt");
        file.DomainEvents.Should().ContainSingle(e => e is FileMetadataUpdatedEvent);
    }
}
