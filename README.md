# Vehicle Component Tracking App

* Service to scan and upload component and serial numbers to Ford.

## run
```
dotnet run --project SKD.VCS.Server
```

## dev database and connections string

Run the following to start/ stop dev db server

```bash
docker-compose  -f docker-compose.dev.yml up -d
docker-compose  -f docker-compose.dev.yml down
```

Mkake sure your `appsettings.json` connection string matches

```json
"server=localhost,9301;database=skd;uid=sa;pwd=DevOnlyPassword119"
```

## running the app

1. ensure db server running
2. run server

```
docker-compose  -f docker-compose.dev.yml up -d
dotnet run --project SKD.VCS.Server
```

## seed with mock data

```
curl https://localhost:5101/seed_mock_data -X POST -d "{}"
```

## Database migration

Install the `dotnet-ef` tooling globally

```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

### Add migrations

```bash
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
