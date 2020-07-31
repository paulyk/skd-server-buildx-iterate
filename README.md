# Vehicle Component Tracking App

* Service to scan and upload component and serial numbers to Ford.
* Shipments Parts Receving Service

## Appsettings

Create at `SKD.VCS.Model/appsettings.json` with the following structure.

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
    "Development": "server=localhost,9301;database=skd;uid=sa;pwd=Resign98",
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
"server=localhost,9301;database=skd;uid=sa;pwd=Resign98"
```

## Running migrations

Install the `dotnet-ef` tooling globally

```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef

```

### Add migrations

```bash
cd SKD.VCS.Model
dotnet ef migrations add <igration-name> -o src/Migrations  --project SKD.VCS.Model 
```

### Remove migratins
```
dotnet ef migrations remove --project SKD.VCS.Model
```
### Update database

```bash
dotnet ef database update --project SKD.VCS.Model
```

### Revert to specific migration 
```
dotnet ef database update Migration_Name  --project SKD.VCS.Model
```
