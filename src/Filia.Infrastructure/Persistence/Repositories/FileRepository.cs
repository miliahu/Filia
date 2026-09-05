using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Filia.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core / PostgreSQL implementation of IFileRepository. This is the only
/// place (besides ApplicationDbContext itself) allowed to know about EF Core
/// query operators - Application only ever sees the plain IFileRepository
/// contract and plain FileItem/PagedResult results.
/// </summary>
public class FileRepository : IFileRepository
{
    private readonly ApplicationDbContext _context;

    public FileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(FileItem file, CancellationToken cancellationToken)
    {
        _context.Files.Add(file);
        return Task.CompletedTask;
    }

    public Task<FileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Files.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

    public async Task<PagedResult<FileItem>> GetPagedAsync(
        string? folderPath,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Files.Where(f => !f.IsDeleted);

        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            query = query.Where(f => f.FolderPath == folderPath);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(f => f.FileName.Contains(searchTerm));
        }

        query = query.OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FileItem>(items, totalCount);
    }
}
