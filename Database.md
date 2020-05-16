# Notes

Example efcore setup with migrations and seeting.

## dev db setup

You need sqlcmd tools

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Resign98" \
   -p 9301:1433 \
   --name vtdb \
   -v vtdb:/var/opt/mssql/data \
   -d mcr.microsoft.com/mssql/server:2017-latest

```

## reset 

```
docker stop vtdb
docker rm vtdb
```

## create db

```bash
docker exec -it vtdb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Resign98 -Q "create database vtdb"

docker exec -it vtdb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Resign98 -Q "select name from sys.databases"
```

## drop container

```bash
docker container stop vtdb
docker container rm vtdb
docker volume rm vtdb
```

## run seed data generator

```bash
docker container start vtdb
export provider="sqlserver"
export connectionString="server=localhost,9301;database=vtdb;uid=sa;pwd=Resign98"
dotnet run --project ./VT.Seed
```

## appsettings

Change to appsettings connection string as to match your db.

```json
{
  "connectionString": "server=localhost,9301;database=vtdb;uid=sa;pwd=Resign98"
}
```

## migrations

Install the migration cli

```bash
dotnet tool install --global dotnet-ef
```

Add remove migrations

```bash
dotnet ef migrations add Added_Entity
dotnet ef migrations remove
```

Apply migration

```bash
dotnet ef database update
```

## util

Drop recreate databasec

```bash
docker exec -it vtdb /opt/mssql-tools/bin/sqlcmd  \ 
-S localhost,9301 -U sa -P Stopangry8 -Q "drop database vtdb ; create database vtdb"
```
