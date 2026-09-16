# Migrations

No migration has been generated inside this archive (no network/dotnet SDK
was available in the environment that produced it). After restoring packages
locally, create the initial migration with:

```bash
dotnet tool install --global dotnet-ef   # once
cd src/Filia.Infrastructure
dotnet ef migrations add InitialCreate \
  --startup-project ../Filia.Api \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --startup-project ../Filia.Api
```

Aspire's Postgres resource will also apply migrations automatically if you
wire up `context.Database.MigrateAsync()` in a startup hook / migration
worker - see `Program.cs` in `Filia.Api` for a commented example.


dotnet ef migrations add InitialCreate --project src/Filia.Infrastructure --startup-project src/Filia.Api --output-dir Persistence/Migrations