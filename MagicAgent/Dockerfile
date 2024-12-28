FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
EXPOSE 9/udp

RUN apk add -U --no-cache tzdata

WORKDIR /app
COPY bin/Release/net8.0/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MagicAgent.dll"]