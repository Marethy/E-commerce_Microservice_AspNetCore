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

# [FIX LỖI QUAN TRỌNG] Copy file quản lý version gói
# Không có file này, các thư viện Serilog/Hosting sẽ không được tải về -> Gây lỗi CS0234, CS0246
COPY ["Directory.Packages.props", "."]

# A. COPY CÁC FILE .CSPROJ VÀ RESTORE
COPY ["src/Services/Customer/Customer.API/Customer.API.csproj", "src/Services/Customer/Customer.API/"]
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus/EventBus.Messages/"]

RUN dotnet restore "src/Services/Customer/Customer.API/Customer.API.csproj"

# B. COPY SOURCE CODE
# 1. Copy source code của chính service Customer
COPY ["src/Services/Customer/Customer.API/", "src/Services/Customer/Customer.API/"]

# 2. Copy source code của các thư viện dùng chung (BuildingBlocks)
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# C. BUILD VÀ PUBLISH
WORKDIR "/microservices/src/Services/Customer/Customer.API"
RUN dotnet build "./Customer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Customer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Customer.API.dll"]