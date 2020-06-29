# Vehicle Component Tracking App

Scan and record specific vehicle components for which vehicle management software must be configured for.

## Appsettings

Create at `SKD.Model/appsettings.json` with the following structure.

This will be used for migrations.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "connectionStrings": {
    "Production": "",
    "Development": "server=localhost,9301;database=vtdb;uid=sa;pwd=Resign98",
    "Staging": ""
  },
  "DatabaseProviderName": "sqlserver"
}
```

## dev database and connections string

Run the following to start/ stop dev db server

```bash
docker-compose  -f docker-compose.dev.yml up -d
docker-compose  -f docker-compose.dev.yml down
```

Mkake sure your `appsettings.json` connection string matches

```json
"server=localhost,9301;database=vtdb;uid=sa;pwd=Resign98"
```

## Running migrations

Install the `dotnet-ef` tooling globally

```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef

```

### Add migrations

```bash
cd SKD.Model
dotnet ef migrations add InitialCreate -o src/Migrations  --project SKD.Model 
dotnet ef migrations add VehicleComponent_Completed --project SKD.Model
dotnet ef migrations add VehicleComponent_Serial_Nullable --project SKD.Model
dotnet ef migrations add VehicleComponent_Sequence --project SKD.Model
dotnet ef migrations add Component_Unique_Name --project SKD.Model
dotnet ef migrations add Component_FordComponentType --project SKD.Model
dotnet ef migrations add Refactor_Component --project SKD.Model
```

### Remove migratins
```
dotnet ef migrations remove --project SKD.Model
```
### Update database

```bash
dotnet ef database update --project SKD.Model
```

### Revert to specific migration 
```
dotnet ef database update Migration_Name  --project SKD.Model
```
