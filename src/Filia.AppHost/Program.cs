using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Non-secret settings come from appsettings.json / appsettings.Development.json
// under the "Filia" section, so the AppHost topology can be tuned
// per-environment without touching this file. Secrets (passwords, RustFS
// secret key) are still modeled as Aspire parameters, which read from
// user-secrets / the Parameters:* configuration section rather than plain
// appsettings - see the "postgres-password" / "rustfs-secret-key" parameters below.
var settings = builder.Configuration.GetSection("Filia");

// PostgreSQL - persistent volume so data survives container restarts across `aspire run`.
var postgresSettings = settings.GetSection("Postgres");
var postgresUsername = builder.AddParameter("postgres-username", postgresSettings["Username"] ?? "postgres");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
    .WithImageTag(postgresSettings["ImageTag"] ?? "16-alpine")
    .WithDataVolume()
    .WithPgAdmin();

var databaseName = postgresSettings["DatabaseName"] ?? "filiadb";
var filesDb = postgres.AddDatabase(databaseName, databaseName);

// RabbitMQ - used for publishing file lifecycle integration events.
var rabbitMqSettings = settings.GetSection("RabbitMq");
var rabbitMqUsername = builder.AddParameter("rabbitmq-username", rabbitMqSettings["Username"] ?? "filia");
var rabbitMqPassword = builder.AddParameter("rabbitmq-password", secret: true);
var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitMqUsername, rabbitMqPassword)
    .WithImageTag(rabbitMqSettings["ImageTag"] ?? "3-management-alpine")
    .WithDataVolume(rabbitMqSettings["DataVolumeName"] ?? "filia-rabbitmq-data")
    .WithManagementPlugin();

// RustFS - S3-compatible object storage backing the file content itself.
// Runs as a plain container resource; the API talks to it over the S3 API
// via the RustFs:* configuration section / AWSSDK.S3 client.
var rustFsSettings = settings.GetSection("RustFs");
var rustFsAccessKey = builder.AddParameter("rustfs-access-key", rustFsSettings["AccessKey"] ?? "rustfs-access-key");
var rustFsSecretKey = builder.AddParameter("rustfs-secret-key", secret: true);

var rustfs = builder.AddContainer("rustfs", rustFsSettings["Image"] ?? "rustfs/rustfs", rustFsSettings["Tag"] ?? "latest")
    .WithEndpoint(port: rustFsSettings.GetValue<int?>("S3Port") ?? 9000, targetPort: 9000, name: "s3")
    .WithEndpoint(port: rustFsSettings.GetValue<int?>("ConsolePort") ?? 9001, targetPort: 9001, name: "console")
    .WithVolume(rustFsSettings["DataVolumeName"] ?? "filia-rustfs-data", "/data")
    .WithEnvironment("RUSTFS_ACCESS_KEY", rustFsAccessKey)
    .WithEnvironment("RUSTFS_SECRET_KEY", rustFsSecretKey);

builder.AddProject<Projects.Filia_Api>("filia-api")
    .WithHttpEndpoint(port: 8080 )
    .WithReference(filesDb)
    .WaitFor(filesDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithEnvironment("RustFs__Endpoint", rustfs.GetEndpoint("s3"))
    .WithEnvironment("RustFs__AccessKey", rustFsAccessKey)
    .WithEnvironment("RustFs__SecretKey", rustFsSecretKey)
    .WaitFor(rustfs);

builder.Build().Run();
