# ---------- dev: used by docker-compose.yml (source mounted, hot reload) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "watch", "run", "--project", "TravelApp.Api/TravelApp.Api.csproj", "--no-launch-profile"]

# ---------- build: compile a release version ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TravelApp.Api/TravelApp.Api.csproj TravelApp.Api/
RUN dotnet restore TravelApp.Api/TravelApp.Api.csproj
COPY . .
RUN dotnet publish TravelApp.Api/TravelApp.Api.csproj -c Release -o /out --no-restore

# ---------- prod: small runtime-only image ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS prod
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "TravelApp.Api.dll"]
