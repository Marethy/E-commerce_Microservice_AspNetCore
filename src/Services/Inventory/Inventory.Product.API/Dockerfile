# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /microservices
COPY ["src/Services/Inventory/Inventory.Product.API/Inventory.Product.API.csproj", "src/Services/Inventory/Inventory.Product.API/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.MessageComponents/EventBus.MessageComponents.csproj", "src/BuildingBlocks/EventBus/EventBus.MessageComponents/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]
RUN dotnet restore "src/Services/Inventory/Inventory.Product.API/Inventory.Product.API.csproj"
COPY . .
WORKDIR "/microservices/src/Services/Inventory/Inventory.Product.API"
RUN dotnet build "./Inventory.Product.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Inventory.Product.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Product.API.dll"]