# Multi-stage Dockerfile for BankManagementSystem (.NET 8.0 Windows Forms Application)
# This application is a Windows Forms desktop app with an embedded health check endpoint
# for containerization support

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy solution file
COPY BankManagementSystem.sln ./

# Copy project files for dependency restoration (optimize layer caching)
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

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0

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
ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=UTC \
    HEALTH_CHECK_PORT=8080 \
    CURRENT_USER=containeruser

# Expose health check port
EXPOSE 8080

# Health check endpoint (application provides /health endpoint)
# Note: This application has an embedded HTTP health check endpoint
# No additional tools (curl/wget) are needed

# Set entry point
ENTRYPOINT ["dotnet", "BankManagementSystem.dll"]
