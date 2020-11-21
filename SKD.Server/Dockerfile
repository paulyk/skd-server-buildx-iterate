FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5100
EXPOSE 5101

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SKD.Server/SKD.Server.csproj", "SKD.Server/"]
RUN dotnet restore "SKD.Server/SKD.Server.csproj"
COPY . .
WORKDIR "/src/SKD.Server"
RUN dotnet build "SKD.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SKD.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SKD.Server.dll"]
