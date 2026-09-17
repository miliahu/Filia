using Amazon.S3;
using Amazon.Runtime;
using Filia.Application.Common.Interfaces;
using Filia.Infrastructure.FileStorage;
using Filia.Infrastructure.Persistence;
using Filia.Infrastructure.Persistence.Interceptors;
using Filia.Infrastructure.Persistence.Repositories;
using Filia.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTime, DateTimeService>();

        // --- Persistence (PostgreSQL via EF Core) ---
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("FiliaDb"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "filia"));
        });

        // Repository pattern: Application only ever sees these two contracts.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IFileRepository, FileRepository>();

        // --- Object storage (RustFS, S3-compatible) ---
        services.Configure<RustFsOptions>(configuration.GetSection(RustFsOptions.SectionName));
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = configuration.GetSection(RustFsOptions.SectionName).Get<RustFsOptions>()
                          ?? throw new InvalidOperationException("RustFs configuration section is missing.");

            var config = new AmazonS3Config
            {
                ServiceURL = options.Endpoint, // برای ارتباط داخلی API
                ForcePathStyle = options.ForcePathStyle,
                AuthenticationRegion = options.Region
            };

            return new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config);
        });

        // کلاینت جداگانه برای presigned URL
        services.AddKeyedSingleton<IAmazonS3>("presign", (sp, _) =>
        {
            var options = configuration.GetSection(RustFsOptions.SectionName).Get<RustFsOptions>()!;
            var publicUrl = options.PublicEndpoint ?? options.Endpoint;

            return new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKey, options.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = publicUrl,
                    ForcePathStyle = options.ForcePathStyle,
                    AuthenticationRegion = options.Region
                });
        });
        services.AddScoped<IFileStorageService, RustFsStorageService>();

        return services;
    }
}