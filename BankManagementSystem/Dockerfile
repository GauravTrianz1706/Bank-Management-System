# Multi-stage Dockerfile for BankManagementSystem (.NET 6 Windows Forms Application)
# This application is a Windows Forms desktop app with health check endpoint for containerization
# Base image: Using explicit base image provided in request

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

# Copy project files for dependency caching
COPY BankManagementSystem/BankManagementSystem.csproj BankManagementSystem/
COPY BankDatabaseAccess/BankDatabaseAccess.csproj BankDatabaseAccess/

# Restore dependencies
RUN dotnet restore BankManagementSystem/BankManagementSystem.csproj

# Copy source code
COPY BankManagementSystem/ BankManagementSystem/
COPY BankDatabaseAccess/ BankDatabaseAccess/

# Build the application
WORKDIR /src/BankManagementSystem
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-build

# Runtime stage - Using explicit base image from request
FROM Regis/Repo/Test

WORKDIR /app

# Create non-root user for security
RUN groupadd -r bankapp && useradd -r -g bankapp bankapp || true

# Copy published application
COPY --from=builder /app/publish .

# Set ownership
RUN chown -R bankapp:bankapp /app || true

# Switch to non-root user
USER bankapp

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    HEALTH_CHECK_PORT=8080 \
    STARTUP_USER=containeruser

# Expose health check port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "BankManagementSystem.dll"]
