using Filia.Application.Common.Interfaces;
using Filia.Domain.Entities;
using Filia.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Filia.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext. Only Infrastructure types (FileRepository, this class)
/// know this exists - Application only sees IFileRepository / IUnitOfWork.
/// </summary>
public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    DispatchDomainEventsInterceptor dispatchDomainEventsInterceptor,
    AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor) : DbContext(options), IUnitOfWork
{
    public DbSet<FileItem> Files => Set<FileItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("filia");
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditableEntitySaveChangesInterceptor, dispatchDomainEventsInterceptor);
    }
}
