# Multi-stage Dockerfile for BankManagementSystem (.NET 6.0 Windows Forms Application)
# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

# Copy project files for dependency caching
COPY BankDatabaseAccess/BankDatabaseAccess.csproj BankDatabaseAccess/
COPY BankManagementSystem/BankManagementSystem.csproj BankManagementSystem/

# Restore dependencies
RUN dotnet restore BankManagementSystem/BankManagementSystem.csproj

# Copy all source code
COPY BankDatabaseAccess/ BankDatabaseAccess/
COPY BankManagementSystem/ BankManagementSystem/

# Build the application
WORKDIR /src/BankManagementSystem
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-build

# Stage 2: Runtime Stage
FROM mcr.microsoft.com/dotnet/runtime:6.0

# Set working directory
WORKDIR /app

# Create non-root user for security
RUN groupadd -r bankapp && useradd -r -g bankapp bankapp

# Copy published application from builder
COPY --from=builder /app/publish .

# Set ownership to non-root user
RUN chown -R bankapp:bankapp /app

# Switch to non-root user
USER bankapp

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    HEALTH_CHECK_PORT=8080 \
    STARTUP_USER=containerized \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose health check port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "BankManagementSystem.dll"]
