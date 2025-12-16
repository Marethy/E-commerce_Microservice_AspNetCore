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
# Lưu ý: Folder này mình giữ là /microservice (số ít) giống file gốc của bạn
WORKDIR /microservice

# [FIX LỖI QUAN TRỌNG] Copy file quản lý version gói
# Nếu thiếu file này -> Không tải được thư viện HealthChecksUI -> Gây lỗi code Program.cs
COPY ["Directory.Packages.props", "."]

# A. COPY FILE .CSPROJ VÀ RESTORE
COPY ["src/WebApps/WebHealthStatus/WebHealthStatus.csproj", "src/WebApps/WebHealthStatus/"]
RUN dotnet restore "src/WebApps/WebHealthStatus/WebHealthStatus.csproj"

# B. COPY SOURCE CODE
COPY ["src/WebApps/WebHealthStatus/", "src/WebApps/WebHealthStatus/"]

# C. BUILD VÀ PUBLISH
WORKDIR "/microservice/src/WebApps/WebHealthStatus"
RUN dotnet build "./WebHealthStatus.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebHealthStatus.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebHealthStatus.dll"]