using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var settings = builder.Configuration.GetSection("Filia");

// ==================== PostgreSQL ====================
var postgresSettings = settings.GetSection("Postgres");
var postgresUsername = builder.AddParameter("postgres-username", postgresSettings["Username"] ?? "postgres");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
    .WithImageTag(postgresSettings["ImageTag"] ?? "16-alpine")
    .WithDataVolume()
    .WithPgAdmin();

var databaseName = postgresSettings["DatabaseName"] ?? "filiadb";
var filesDb = postgres.AddDatabase(databaseName, databaseName);

// ==================== RustFS ====================
var rustFsSettings = settings.GetSection("RustFs");
var rustFsAccessKey = builder.AddParameter("rustfs-access-key", rustFsSettings["AccessKey"] ?? "rustfs-access-key");
var rustFsSecretKey = builder.AddParameter("rustfs-secret-key", secret: true);

var rustfs = builder.AddContainer("rustfs", rustFsSettings["Image"] ?? "rustfs/rustfs", rustFsSettings["Tag"] ?? "latest")
    .WithEndpoint(port: rustFsSettings.GetValue<int?>("S3Port") ?? 9000, targetPort: 9000, name: "s3")
    .WithEndpoint(port: rustFsSettings.GetValue<int?>("ConsolePort") ?? 9001, targetPort: 9001, name: "console")
    .WithVolume(rustFsSettings["DataVolumeName"] ?? "filia-rustfs-data", "/data")
    .WithEnvironment("RUSTFS_ACCESS_KEY", rustFsAccessKey)
    .WithEnvironment("RUSTFS_SECRET_KEY", rustFsSecretKey);
 
builder.AddProject<Filia_Api>("filia-api")
    .WithHttpEndpoint(port: 8080)
    .WithReference(filesDb)
    .WaitFor(filesDb)
    .WithEnvironment("RustFs__Endpoint", rustFsSettings["PublicEndpoint"]) 
    .WithEnvironment("RustFs__PublicEndpoint", rustFsSettings["PublicEndpoint"]) 
    .WithEnvironment("RustFs__AccessKey", rustFsAccessKey)
    .WithEnvironment("RustFs__SecretKey", rustFsSecretKey)
    .WaitFor(rustfs);

builder.Build().Run();