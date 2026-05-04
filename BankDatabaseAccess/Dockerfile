# Multi-stage Dockerfile for BankDatabaseAccess (.NET 8.0)
# This Dockerfile builds the BankDatabaseAccess library as part of the BankManagementSystem solution

# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy solution file
COPY BankManagementSystem.sln ./

# Copy project files for dependency caching
COPY BankDatabaseAccess/BankDatabaseAccess.csproj ./BankDatabaseAccess/
COPY BankManagementSystem/BankManagementSystem.csproj ./BankManagementSystem/

# Restore dependencies
RUN dotnet restore BankManagementSystem.sln

# Copy all source code
COPY BankDatabaseAccess/ ./BankDatabaseAccess/
COPY BankManagementSystem/ ./BankManagementSystem/

# Build the solution
RUN dotnet build BankManagementSystem.sln -c Release --no-restore

# Publish the application
RUN dotnet publish BankManagementSystem/BankManagementSystem.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-build

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Set working directory
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application from builder
COPY --from=builder /app/publish .

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production \
    HEALTH_CHECK_PORT=8080 \
    CURRENT_USER=containeruser \
    TZ=UTC

# Create logs directory
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose health check port
EXPOSE 8080

# Health check endpoint (application provides /health endpoint)
# Note: Container health checks rely on the application's built-in health endpoint

# Entry point
ENTRYPOINT ["dotnet", "BankManagementSystem.dll"]
