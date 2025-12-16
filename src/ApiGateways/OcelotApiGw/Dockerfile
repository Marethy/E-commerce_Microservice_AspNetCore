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
COPY ["src/ApiGateways/OcelotApiGw/OcelotApiGw.csproj", "src/ApiGateways/OcelotApiGw/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]

# [FIX LỖI] Bổ sung EventBus.Messages vì log báo thiếu
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]

RUN dotnet restore "./src/ApiGateways/OcelotApiGw/OcelotApiGw.csproj"

# B. COPY SOURCE CODE
# 1. Copy source code của Ocelot
COPY ["src/ApiGateways/OcelotApiGw/", "src/ApiGateways/OcelotApiGw/"]

# 2. Copy source code của các thư viện dùng chung
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# C. BUILD VÀ PUBLISH
WORKDIR "/microservices/src/ApiGateways/OcelotApiGw"
RUN dotnet build "./OcelotApiGw.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OcelotApiGw.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcelotApiGw.dll"]