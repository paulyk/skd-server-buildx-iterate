# Vehicle Component Tracking App

Scan and record specific vehicle components for which vehicle management software must be configured for.

## Appsettings

Create at `VT.Model/appsettings.json` with the following structure.

This will be used for migrations.

```json
{
  "provider": "sqlite",
  "connectionString": "Data Source=/path-to-db/<dbname>.db"
}
```

Create at `VT.Seed/appsettings.json` with the following structure.

This will be used for seeding the database for dev purposes.

```json
{
  "provider": "sqlite",
  "connectionString": "Data Source=/path-to-db/<dbname>.db"
}
```

## Running migrations

Install the `dotnet-ef` tooling globally

```bash
dotnet tool install --global dotnet-ef
```

Initial migration 

```bash
cd VT.Model
dotnet ef migrations add InitialCreate -o src/Migrations      
```

Update database

```bash
dotnet ef database update
```