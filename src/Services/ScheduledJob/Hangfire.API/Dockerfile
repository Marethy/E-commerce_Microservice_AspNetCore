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

# Copy file quản lý version gói (cho đồng bộ với các service khác)
COPY ["Directory.Packages.props", "."]

# A. COPY CÁC FILE .CSPROJ VÀ RESTORE
COPY ["src/Services/ScheduledJob/Hangfire.API/Hangfire.API.csproj", "src/Services/ScheduledJob/Hangfire.API/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]

# [FIX LỖI] Bổ sung EventBus.Messages vì log báo thiếu
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]

RUN dotnet restore "src/Services/ScheduledJob/Hangfire.API/Hangfire.API.csproj"

# B. COPY SOURCE CODE
# 1. Copy source code của chính service Hangfire
COPY ["src/Services/ScheduledJob/Hangfire.API/", "src/Services/ScheduledJob/Hangfire.API/"]

# 2. Copy source code của các thư viện dùng chung (BuildingBlocks)
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# C. BUILD VÀ PUBLISH
WORKDIR "/microservices/src/Services/ScheduledJob/Hangfire.API"
RUN dotnet build "./Hangfire.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hangfire.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hangfire.API.dll"]