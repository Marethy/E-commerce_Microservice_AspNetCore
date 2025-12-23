# Stage 1: Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /microservices

# Copy solution and project files
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY ["microservice.sln", "."]

# Copy all project files
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.MessageComponents/EventBus.MessageComponents.csproj", "src/BuildingBlocks/EventBus/EventBus.MessageComponents/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/Services/Payment/Payment.API/Payment.API.csproj", "src/Services/Payment/Payment.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/Payment/Payment.API/Payment.API.csproj"

# Copy source code
COPY ["src/BuildingBlocks/Common.Logging/", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.MessageComponents/", "src/BuildingBlocks/EventBus/EventBus.MessageComponents/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/", "src/BuildingBlocks/EventBus/EventBus.Messages/"]
COPY ["src/BuildingBlocks/Infrastructure/", "src/BuildingBlocks/Infrastructure/"]
COPY ["src/BuildingBlocks/Shared/", "src/BuildingBlocks/Shared/"]
COPY ["src/Services/Payment/Payment.API/", "src/Services/Payment/Payment.API/"]

# Build
WORKDIR "/microservices/src/Services/Payment/Payment.API"
RUN dotnet build "./Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.API.dll"]
