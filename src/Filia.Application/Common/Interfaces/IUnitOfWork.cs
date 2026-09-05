namespace Filia.Application.Common.Interfaces;

/// <summary>
/// Commits the changes made through IFileRepository (and any other
/// repositories added later) as a single transaction. Kept separate from
/// IFileRepository so a use case that touches multiple repositories still
/// saves everything atomically through one call.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
