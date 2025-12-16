# ---------------------------------------------
# Stage 1: Base
# ---------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------------------------------------------
# Stage 2: Build
# ---------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /microservices

# [BẮT BUỘC] Copy file quản lý version gói
# Nếu thiếu dòng này -> Lỗi thiếu thư viện MassTransit, Serilog
COPY ["Directory.Packages.props", "."]

# A. COPY CÁC FILE .CSPROJ VÀ RESTORE
COPY ["src/Services/Inventory/Inventory.Product.API/Inventory.Product.API.csproj", "src/Services/Inventory/Inventory.Product.API/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.MessageComponents/EventBus.MessageComponents.csproj", "src/BuildingBlocks/EventBus/EventBus.MessageComponents/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]

RUN dotnet restore "src/Services/Inventory/Inventory.Product.API/Inventory.Product.API.csproj"

# B. COPY SOURCE CODE
# 1. Copy source code của chính service Inventory
COPY ["src/Services/Inventory/Inventory.Product.API/", "src/Services/Inventory/Inventory.Product.API/"]

# 2. Copy source code của các thư viện dùng chung (BuildingBlocks)
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# C. BUILD VÀ PUBLISH
WORKDIR "/microservices/src/Services/Inventory/Inventory.Product.API"
RUN dotnet build "./Inventory.Product.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Inventory.Product.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Product.API.dll"]