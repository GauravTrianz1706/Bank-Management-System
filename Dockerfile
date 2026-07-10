# Dockerfile for Bank Management System
# Multi-stage build for .NET Framework application

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app

# Copy project files
COPY BankDatabaseAccess/BankDatabaseAccess.csproj ./BankDatabaseAccess/
COPY BankManagementSystem/BankManagementSystem.csproj ./BankManagementSystem/

# Restore dependencies
RUN nuget restore BankDatabaseAccess/BankDatabaseAccess.csproj
RUN nuget restore BankManagementSystem/BankManagementSystem.csproj

# Copy source code
COPY BankDatabaseAccess/ ./BankDatabaseAccess/
COPY BankManagementSystem/ ./BankManagementSystem/

# Build application
RUN msbuild BankManagementSystem/BankManagementSystem.csproj /p:Configuration=Release /p:OutputPath=/app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8
WORKDIR /app

# Copy built application
COPY --from=build /app/out .

# Environment variables for containerization
ENV DB_CONNECTION_STRING="" \
    DB_HOST="localhost" \
    DB_NAME="OpenBankLocal" \
    DB_USER="sa" \
    DB_PASSWORD="" \
    CURRENT_USER="container-user" \
    STARTUP_USER="container-user" \
    AUDIT_LOG_PATH="/app/logs/audit" \
    CUSTOMER_LOG_PATH="/app/logs/customer" \
    DEPOSIT_CACHE_PATH="/app/cache/deposit" \
    BANK_APP_CONFIG="production" \
    HEALTH_CHECK_PORT="8080"

# Create required directories
RUN mkdir -p /app/logs/audit /app/logs/customer /app/cache/deposit

# Expose health check port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD powershell -Command "try { Invoke-WebRequest -Uri http://localhost:8080/health -UseBasicParsing | Out-Null; exit 0 } catch { exit 1 }"

# Run application
ENTRYPOINT ["BankManagementSystem.exe"]
