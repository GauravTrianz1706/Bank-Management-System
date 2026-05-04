# Containerization Configuration Guide

## Environment Variables

This application has been containerized and requires the following environment variables to be configured:

### Database Configuration
- `DB_CONNECTION_STRING`: Full database connection string (optional if individual components are provided)
- `DB_HOST`: Database server hostname (e.g., `localhost`, `db.example.com`)
- `DB_NAME`: Database name (e.g., `BankDB`)
- `DB_USER`: Database username
- `DB_PASSWORD`: Database password

### File System Paths
All file paths have been externalized to support cross-platform deployment:

- `AUDIT_LOG_PATH`: Path for employee audit logs (default: `{temp}/EmployeeAudit/session.log`)
- `CUSTOMER_LOG_PATH`: Path for customer access logs (default: `{temp}/CustomerLogs/access.log`)
- `DEPOSIT_CACHE_PATH`: Path for deposit cache files (default: `{temp}/DepositCache/last.txt`)

### Authentication & User Management
- `CURRENT_USER`: Current user identifier (replaces Windows Authentication)
- `BANK_APP_SETTINGS`: Application settings JSON (replaces Windows Registry)

### Health Check Configuration
- `HEALTH_CHECK_PORT`: Port for health check endpoint (default: `8080`)

## Health Check Endpoint

The application exposes HTTP health check endpoints for container orchestration:

- `http://localhost:8080/health/` - Health check endpoint
- `http://localhost:8080/healthz/` - Alternative health check endpoint

Response format:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00.0000000Z",
  "application": "BankManagementSystem",
  "version": "1.0.0"
}
```

## Docker Example

```dockerfile
FROM mcr.microsoft.com/dotnet/framework/runtime:4.7.2

# Set environment variables
ENV DB_HOST=sqlserver \
    DB_NAME=BankDB \
    DB_USER=sa \
    DB_PASSWORD=YourPassword123 \
    HEALTH_CHECK_PORT=8080 \
    CURRENT_USER=container_user

# Copy application files
COPY . /app
WORKDIR /app

# Expose health check port
EXPOSE 8080

# Run application
ENTRYPOINT ["BankManagementSystem.exe"]
```

## Kubernetes Example

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: bank-management-system
spec:
  containers:
  - name: app
    image: bank-management-system:latest
    env:
    - name: DB_HOST
      value: "sqlserver-service"
    - name: DB_NAME
      value: "BankDB"
    - name: DB_USER
      valueFrom:
        secretKeyRef:
          name: db-credentials
          key: username
    - name: DB_PASSWORD
      valueFrom:
        secretKeyRef:
          name: db-credentials
          key: password
    - name: HEALTH_CHECK_PORT
      value: "8080"
    ports:
    - containerPort: 8080
      name: health
    livenessProbe:
      httpGet:
        path: /health/
        port: 8080
      initialDelaySeconds: 30
      periodSeconds: 10
    readinessProbe:
      httpGet:
        path: /health/
        port: 8080
      initialDelaySeconds: 5
      periodSeconds: 5
```

## Changes Made for Containerization

### 1. Windows-Specific Paths (cz-dotnet-0001)
- Replaced hardcoded Windows paths (C:\, D:\) with environment variables
- Added cross-platform path construction using `Path.Combine()` and `Path.GetTempPath()`

### 2. Registry Access (cz-dotnet-0002)
- Removed `Microsoft.Win32.Registry` API calls
- Replaced with environment variable-based configuration

### 3. Drive Letter Dependencies (cz-dotnet-0006)
- Eliminated all hardcoded drive letter references
- Implemented environment variable-based path configuration

### 4. Windows Authentication (cz-dotnet-0036)
- Removed `WindowsIdentity.GetCurrent()` calls
- Replaced with environment variable-based user identification

### 5. Web.config Transforms (cz-dotnet-0055)
- Replaced `ConfigurationManager.ConnectionStrings` with environment variables
- Configuration now supports container-based deployment

## Migration Notes

1. **Database Connection**: Ensure your database is accessible from the container network
2. **File Permissions**: Ensure the container has write permissions to configured paths
3. **Health Check**: The health check service runs on a background thread and doesn't interfere with the main application
4. **Windows Forms**: This is a Windows Forms application that requires a Windows container base image
