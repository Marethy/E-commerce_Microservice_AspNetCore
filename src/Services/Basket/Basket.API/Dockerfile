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

# [CHỈ GIỮ LẠI FILE NÀY] Copy file quản lý version gói
COPY ["Directory.Packages.props", "."]
# Đã xóa dòng COPY Directory.Build.props gây lỗi

# A. COPY CÁC FILE .CSPROJ VÀ RESTORE
COPY ["src/Services/Basket/Basket.API/Basket.API.csproj", "src/Services/Basket/Basket.API/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.MessageComponents/EventBus.MessageComponents.csproj", "src/BuildingBlocks/EventBus/EventBus.MessageComponents/"]

# Copy Inventory.Grpc và Protos (Basket cần cái này)
COPY ["src/Services/Inventory/Inventory.Grpc/Inventory.Grpc.csproj", "src/Services/Inventory/Inventory.Grpc/"]
COPY ["src/BuildingBlocks/Contracts/Protos", "src/BuildingBlocks/Contracts/Protos"]

RUN dotnet restore "src/Services/Basket/Basket.API/Basket.API.csproj"

# B. COPY SOURCE CODE
# 1. Copy source code của chính service Basket
COPY ["src/Services/Basket/Basket.API/", "src/Services/Basket/Basket.API/"]

# 2. Copy source code của các thư viện dùng chung (BuildingBlocks)
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# 3. Copy source code của Inventory.Grpc
COPY ["src/Services/Inventory/Inventory.Grpc/", "src/Services/Inventory/Inventory.Grpc/"]

# C. BUILD VÀ PUBLISH
WORKDIR "/microservices/src/Services/Basket/Basket.API"
RUN dotnet build "./Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Basket.API.dll"]