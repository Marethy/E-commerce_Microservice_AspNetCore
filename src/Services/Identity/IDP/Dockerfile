# Base stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Directory.Packages.props (central package management)
COPY ["Directory.Packages.props", "."]

# A. COPY PROJECT FILES (.CSPROJ) & RESTORE
# Giữ nguyên phần này để tận dụng cache của NuGet packages
COPY ["src/Services/Identity/IDP/IDP.csproj", "src/Services/Identity/IDP/"]
COPY ["src/Services/Identity/Infrastructure/IDP.Infrastructure.csproj", "src/Services/Identity/Infrastructure/"]
COPY ["src/Services/Identity/Presentation/IDP.Presentation.csproj", "src/Services/Identity/Presentation/"]
COPY ["src/Services/Identity/Shared/Shared.csproj", "src/Services/Identity/Shared/"]

# Copy BuildingBlocks dependencies
COPY ["src/BuildingBlocks/Contracts/Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Shared/Shared.csproj", "src/BuildingBlocks/Shared/"]
COPY ["src/BuildingBlocks/Infrastructure/Infrastructure.csproj", "src/BuildingBlocks/Infrastructure/"]
COPY ["src/BuildingBlocks/Common.Logging/Common.Logging.csproj", "src/BuildingBlocks/Common.Logging/"]

# Restore dependencies
RUN dotnet restore "src/Services/Identity/IDP/IDP.csproj"

# B. COPY SOURCE CODE (ĐÃ SỬA)
# Thay vì "COPY . ." (copy toàn bộ repo), chỉ copy source code của Identity và BuildingBlocks.

# 1. Copy toàn bộ source code liên quan đến Identity (Bao gồm IDP, Infra, Presentation, Shared của Identity)
COPY ["src/Services/Identity/", "src/Services/Identity/"]

# 2. Copy source code của BuildingBlocks (Thư viện dùng chung)
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# Build project
WORKDIR "/src/src/Services/Identity/IDP"
RUN dotnet build "IDP.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "IDP.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IDP.dll"]