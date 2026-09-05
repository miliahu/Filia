using Filia.Api.Middleware;
using Filia.Application;
using Filia.Infrastructure;
using Filia.Infrastructure.Persistence;
using Filia.ServiceDefaults;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// .NET Aspire: service discovery, health checks, OpenTelemetry, resilience.
builder.AddServiceDefaults();

// Serilog reads its configuration from appsettings.json ("Serilog" section)
// and enriches every log line with useful context.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "File Management API",
        Version = "v1",
        Description = "Clean-architecture microservice for uploading, listing and downloading files backed by RustFS."
    });
});

// Clean Architecture composition root: Application (use cases) + Infrastructure (EF Core/Postgres, RustFS, RabbitMQ).
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Aspire wires the "postgresdb" resource's connection string into configuration
// automatically when this project references the Postgres resource in AppHost.
builder.AddNpgsqlDbContext<ApplicationDbContext>("filiadb");

var app = builder.Build();

app.MapDefaultEndpoints(); // Aspire health/liveness endpoints

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Uncomment to apply EF Core migrations automatically on startup (handy for
// local/dev and container environments; prefer a dedicated migration job in prod):
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     await db.Database.MigrateAsync();
// }

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
