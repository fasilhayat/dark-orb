FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app

# The publish output is built on the host via `dotnet publish` before Docker runs.
# This avoids NuGet restore network issues inside isolated build containers.
COPY publish-demo/ .

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "BattleArena.Demo.dll"]
