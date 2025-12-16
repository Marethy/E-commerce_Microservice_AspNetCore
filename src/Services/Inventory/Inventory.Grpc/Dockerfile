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
COPY ["Directory.Packages.props", "."]

# A. COPY CÁC FILE .CSPROJ VÀ RESTORE
COPY ["src/Services/Inventory/Inventory.Grpc/Inventory.Grpc.csproj", "src/Services/Inventory/Inventory.Grpc/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]

# [FIX LỖI] Bổ sung các file EventBus còn thiếu
COPY ["src/BuildingBlocks/EventBus/EventBus.MessageComponents/EventBus.MessageComponents.csproj", "src/BuildingBlocks/EventBus/EventBus.MessageComponents/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]

# Copy Protos & WebHealthStatus
COPY ["src/BuildingBlocks/Contracts/Protos", "src/BuildingBlocks/Contracts/Protos"]
COPY ["src/WebApps/WebHealthStatus/WebHealthStatus.csproj", "src/WebApps/WebHealthStatus/"]

RUN dotnet restore "src/Services/Inventory/Inventory.Grpc/Inventory.Grpc.csproj"

# B. COPY SOURCE CODE
# 1. Copy source code của chính service Inventory.Grpc
COPY ["src/Services/Inventory/Inventory.Grpc/", "src/Services/Inventory/Inventory.Grpc/"]

# 2. Copy source code của các thư viện dùng chung (BuildingBlocks)
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# 3. Copy source code WebHealthStatus
COPY ["src/WebApps/WebHealthStatus/", "src/WebApps/WebHealthStatus/"]

# C. BUILD VÀ PUBLISH
WORKDIR "/microservices/src/Services/Inventory/Inventory.Grpc"
RUN dotnet build "./Inventory.Grpc.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Inventory.Grpc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Grpc.dll"]