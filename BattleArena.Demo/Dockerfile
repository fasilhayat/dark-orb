# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files first for layer caching
COPY BattleArena.Core/BattleArena.Core.csproj           BattleArena.Core/
COPY BattleArena.Application/BattleArena.Application.csproj BattleArena.Application/
COPY BattleArena.Demo/BattleArena.Demo.csproj           BattleArena.Demo/

RUN dotnet restore BattleArena.Demo/BattleArena.Demo.csproj

# Copy source code and publish
COPY BattleArena.Core/       BattleArena.Core/
COPY BattleArena.Application/ BattleArena.Application/
COPY BattleArena.Demo/       BattleArena.Demo/

RUN dotnet publish BattleArena.Demo/BattleArena.Demo.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# API URL injected by docker-compose; demo falls back to local chars if unset
ENV BATTLE_ARENA_API_URL=""
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "BattleArena.Demo.dll"]
