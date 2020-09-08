FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["SKD.VCS.Server/SKD.VCS.Server.csproj", "SKD.VCS.Server/"]
RUN dotnet restore "SKD.VCS.Server/SKD.VCS.Server.csproj"
COPY . .
WORKDIR "/src/SKD.VCS.Server"
RUN dotnet build "SKD.VCS.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SKD.VCS.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SKD.VCS.Server.dll"]
