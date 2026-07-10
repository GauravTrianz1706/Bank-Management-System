# Multi-stage Dockerfile for BankDatabaseAccess (.NET 6.0 Class Library)
# This is a class library project - typically not containerized independently
# This Dockerfile is provided for reference but should be used with the main application

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

# Copy project file for dependency caching
COPY BankDatabaseAccess/BankDatabaseAccess.csproj BankDatabaseAccess/

# Restore dependencies
RUN dotnet restore BankDatabaseAccess/BankDatabaseAccess.csproj

# Copy source code
COPY BankDatabaseAccess/ BankDatabaseAccess/

# Build the library
WORKDIR /src/BankDatabaseAccess
RUN dotnet build BankDatabaseAccess.csproj -c Release -o /app/build

# Publish stage
FROM builder AS publish
RUN dotnet publish BankDatabaseAccess.csproj -c Release -o /app/publish --no-restore

# Note: Class libraries are not typically run independently
# This library should be referenced by the main application (BankManagementSystem)
# For deployment, use the main application's Dockerfile which includes this library
