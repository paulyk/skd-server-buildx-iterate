# RMA SKD Middlware Graphql Server

Record and post component sseiral data to Fords "Data Collection Web Service"

## Projects

* skd.server
* skd.model
* skd.dcws
* skd.test
* skd.seed
* skd.test

## run

```bash
dotnet run --project SKD.Server
```

## dev database and connections string

Run the following to start/ stop dev db server

```bash
docker-compose  -f docker-compose.dev.yml up -d
docker-compose  -f docker-compose.dev.yml down
```

Create your own `developer.json`

Each developer should have their own version of this `developer.json`

```json
{
    "ConnectionStrings": {
        "Default": "server=localhost,9301;database=skd;uid=sa;pwd=DevOnlyPassword119"
    }
}
```

## running the app

1. ensure db server running
2. run server

```bash
docker-compose  -f docker-compose.dev.yml up -d
dotnet run --project SKD.Server
```

## seed with mock data

```bash
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
dotnet ef migrations add <igration-name> -o src/Migrations  --project skd.server 
```

### Remove migratins

```bash
dotnet ef migrations remove --project skd.server
```

### Update database

```bash
dotnet ef database update --project skd.server
dotnet ef database update --project skd.server --connection your_connection_string
```

### Revert to specific migration

```bash
dotnet ef database update Migration_Name --project skd.server
dotnet ef database update Migration_Name --connection your_connection_string

dotnet ef database update --project skd.server
dotnet ef database update --project skd.server --connection "target connection string"
```

### List migrations

```bash
dotnet ef migrations list --project skd.server
```
