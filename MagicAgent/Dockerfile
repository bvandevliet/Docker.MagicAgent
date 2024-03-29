FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app
EXPOSE 9/udp

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet restore "MagicAgent/MagicAgent.csproj"

WORKDIR "/src/MagicAgent"
ARG MODE=Release
RUN dotnet build "MagicAgent.csproj" -c ${MODE} -o /app/build

FROM build AS publish
ARG MODE=Release
RUN dotnet publish "MagicAgent.csproj" -c ${MODE} -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apk add -U --no-cache tzdata
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MagicAgent.dll"]