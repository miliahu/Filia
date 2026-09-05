using Filia.Domain.Entities;

namespace Filia.Application.Common.Interfaces;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);

/// <summary>
/// Repository abstraction for FileItem persistence. Application depends only
/// on this plain, EF-agnostic contract; the EF Core implementation lives in
/// Infrastructure (see FileRepository).
/// </summary>
public interface IFileRepository
{
    /// <summary>Stages a new file for insertion. Call IUnitOfWork.SaveChangesAsync to persist it.</summary>
    Task AddAsync(FileItem file, CancellationToken cancellationToken);

    Task<FileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<FileItem>> GetPagedAsync(
        string? folderPath,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
