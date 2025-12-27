# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["StackFood.Customers.sln", "./"]
COPY ["src/StackFood.Customers.Domain/StackFood.Customers.Domain.csproj", "src/StackFood.Customers.Domain/"]
COPY ["src/StackFood.Customers.Application/StackFood.Customers.Application.csproj", "src/StackFood.Customers.Application/"]
COPY ["src/StackFood.Customers.Infrastructure/StackFood.Customers.Infrastructure.csproj", "src/StackFood.Customers.Infrastructure/"]
COPY ["src/StackFood.Customers.API/StackFood.Customers.API.csproj", "src/StackFood.Customers.API/"]
COPY ["tests/StackFood.Customers.Tests/StackFood.Customers.Tests.csproj", "tests/StackFood.Customers.Tests/"]

# Restore dependencies
RUN dotnet restore "StackFood.Customers.sln"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/src/StackFood.Customers.API"
RUN dotnet build "StackFood.Customers.API.csproj" -c Release -o /app/build
RUN dotnet publish "StackFood.Customers.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8084

# Copy published files
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8084/health || exit 1

ENTRYPOINT ["dotnet", "StackFood.Customers.API.dll"]
