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

# Copy project files with correct restructured paths
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

# Copy everything else
COPY . .

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