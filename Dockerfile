# See https://aka.ms/containerfastmode for faster debugging

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

COPY ["BattleArena.Api/BattleArena.Api.csproj", "BattleArena.Api/"]
COPY ["BattleArena.Application/BattleArena.Application.csproj", "BattleArena.Application/"]
COPY ["BattleArena.Core/BattleArena.Core.csproj", "BattleArena.Core/"]
COPY ["BattleArena.Infrastructure/BattleArena.Infrastructure.csproj", "BattleArena.Infrastructure/"]
RUN dotnet restore "BattleArena.Api/BattleArena.Api.csproj"

COPY . ./
RUN dotnet publish "BattleArena.Api/BattleArena.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
EXPOSE 8585

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8585
ENTRYPOINT ["dotnet", "BattleArena.Api.dll"]
