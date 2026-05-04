# Multi-stage Dockerfile for .NET 6.0 Windows Forms Application
# This application requires Windows containers due to Windows Forms dependency

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-windowsservercore-ltsc2022 AS builder

WORKDIR /src

# Copy solution and project files for dependency caching
COPY BankManagementSystem.sln ./
COPY BankDatabaseAccess/BankDatabaseAccess.csproj ./BankDatabaseAccess/
COPY BankManagementSystem/BankManagementSystem.csproj ./BankManagementSystem/

# Restore dependencies
RUN dotnet restore BankManagementSystem.sln

# Copy the rest of the source code
COPY . .

# Build the solution
WORKDIR /src/BankManagementSystem
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-build --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:6.0-windowsservercore-ltsc2022

WORKDIR /app

# Copy published application
COPY --from=builder /app/publish .

# Set environment variables
ENV HEALTH_CHECK_PORT=8080 \
    STARTUP_USER=containeruser \
    DOTNET_RUNNING_IN_CONTAINER=true

# Expose health check port
EXPOSE 8080

# Create a non-admin user for security
RUN net user /add appuser && \
    icacls "C:\app" /grant appuser:F /T

USER appuser

# Set the entry point
ENTRYPOINT ["BankManagementSystem.exe"]
