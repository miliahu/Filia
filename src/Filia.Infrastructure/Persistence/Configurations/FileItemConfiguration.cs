using Filia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Filia.Infrastructure.Persistence.Configurations;

public class FileItemConfiguration : IEntityTypeConfiguration<FileItem>
{
    public void Configure(EntityTypeBuilder<FileItem> builder)
    {
        builder.ToTable("files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(f => f.ContentType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.StorageBucket)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(f => f.StoragePath)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(f => f.Checksum)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(f => f.FolderPath)
            .HasMaxLength(1024);

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(f => f.FolderPath);
        builder.HasIndex(f => f.IsDeleted);
        builder.HasIndex(f => f.CreatedAt);

        builder.Ignore(f => f.DomainEvents);
    }
}
