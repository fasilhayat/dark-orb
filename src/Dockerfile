FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
EXPOSE 8585

# The publish output is built on the host via `dotnet publish` before Docker runs.
# This avoids NuGet restore network issues inside isolated build containers.
COPY publish/ .
ENV ASPNETCORE_URLS=http://+:8585
ENTRYPOINT ["dotnet", "BattleArena.Api.dll"]
