# Bank Management System - Containerization Guide

## Overview
This document describes the containerization fixes applied to the Bank Management System to enable deployment in container environments.

## Blockers Fixed

### 1. Windows-Specific Paths (cz-dotnet-0001)
**Files Modified:**
- `BankManagementSystem/EmployeeDashBoard.cs` (line 28)
- `BankManagementSystem/EmployeeDashboardForms/CustomerInfo.cs` (line 19)
- `BankManagementSystem/EmployeeDashboardForms/Deposit.cs` (line 22)

**Changes:**
- Replaced hardcoded Windows paths (C:\, D:\) with cross-platform Path APIs
- Used environment variables for configurable paths
- Added directory creation logic to ensure paths exist

### 2. Registry Access (cz-dotnet-0002)
**Files Modified:**
- `BankManagementSystem/CustomerDashBoard.cs` (lines 2, 21)

**Changes:**
- Removed Microsoft.Win32.Registry namespace import
- Replaced Registry.CurrentUser.OpenSubKey() with environment variable configuration
- Configuration should be loaded from AWS Systems Manager Parameter Store

### 3. Drive Letter Dependencies (cz-dotnet-0006)
**Files Modified:**
- `BankManagementSystem/EmployeeDashBoard.cs` (line 28)
- `BankManagementSystem/EmployeeDashboardForms/CustomerInfo.cs` (line 19)
- `BankManagementSystem/EmployeeDashboardForms/Deposit.cs` (line 22)

**Changes:**
- Eliminated hardcoded drive letters (C:, D:)
- Used environment variables for all file paths
- Implemented cross-platform path construction using System.IO.Path

### 4. Windows Authentication (cz-dotnet-0036)
**Files Modified:**
- `BankManagementSystem/EmployeeDashBoard.cs` (line 25)
- `BankManagementSystem/Program.cs` (line 9)

**Changes:**
- Removed WindowsIdentity.GetCurrent() calls
- Replaced with environment variable CURRENT_USER
- Prepared for migration to Amazon Cognito for production

### 5. Web.config Transforms (cz-dotnet-0055)
**Files Modified:**
- `BankDatabaseAccess/DatabaseConnection.cs` (line 12)

**Changes:**
- Replaced ConfigurationManager.ConnectionStrings with environment variable
- Connection string now loaded from DB_CONNECTION_STRING environment variable
- Supports AWS Secrets Manager integration

## Health Check Endpoint

**New Files:**
- `BankManagementSystem/HealthCheckEndpoint.cs`

**Features:**
- HTTP listener on configurable port (default: 8080)
- Endpoint: GET /health
- Returns JSON status: {"status":"UP","timestamp":"...","application":"BankManagementSystem","version":"1.0.0"}
- Integrated into Program.cs for automatic startup

## Environment Variables

The following environment variables must be configured for containerized deployment:

| Variable | Description | Default |
|----------|-------------|---------|
| CURRENT_USER | Current user identifier | anonymous |
| AUDIT_LOG_PATH | Path for audit logs | /tmp/EmployeeAudit |
| CUSTOMER_LOGS_PATH | Path for customer logs | /tmp/CustomerLogs |
| DEPOSIT_CACHE_PATH | Path for deposit cache | /tmp/DepositCache |
| BANK_APP_CONFIG | Application configuration | default |
| HEALTH_CHECK_PORT | Health check endpoint port | 8080 |
| DB_CONNECTION_STRING | Database connection string | (see below) |
| DB_HOST | Database host | - |
| DB_NAME | Database name | - |
| DB_USER | Database user | - |
| DB_PASSWORD | Database password | - |

## Docker Deployment

### Build Image
```bash
docker build -t bank-management-system:latest .
```

### Run Container
```bash
docker run -d \
  -p 8080:8080 \
  -e DB_HOST=sqlserver.example.com \
  -e DB_NAME=OpenBankLocal \
  -e DB_USER=bankuser \
  -e DB_PASSWORD=secure_password \
  -e CURRENT_USER=container_user \
  bank-management-system:latest
```

### Health Check
```bash
curl http://localhost:8080/health
```

## AWS Deployment Recommendations

### 1. Database Connection
- Store connection string in AWS Secrets Manager
- Use IAM roles for authentication where possible
- Reference: `arn:aws:secretsmanager:region:account:secret:bank-db-connection`

### 2. Configuration Management
- Use AWS Systems Manager Parameter Store for non-sensitive configuration
- Use AWS Secrets Manager for sensitive data (passwords, API keys)

### 3. Container Orchestration
- Deploy on Amazon ECS with Fargate (Windows containers)
- Use Application Load Balancer for health checks
- Configure auto-scaling based on CPU/memory metrics

### 4. Logging
- Configure CloudWatch Logs for centralized logging
- Use structured logging for better searchability
- Set appropriate log retention policies

## Migration Notes

### Windows Forms Limitations
This is a Windows Forms desktop application, which has limitations in containerized environments:
- Requires Windows containers (not Linux)
- Interactive UI may not work in headless container environments
- Consider migrating to ASP.NET Core Web API for better container support

### Future Improvements
1. Migrate to ASP.NET Core for Linux container support
2. Implement proper authentication with Amazon Cognito
3. Replace file-based logging with CloudWatch Logs
4. Implement distributed caching with Amazon ElastiCache
5. Add comprehensive health checks (database connectivity, external dependencies)

## Testing

### Local Testing
1. Set environment variables in your development environment
2. Run the application and verify health check endpoint
3. Test file operations with configured paths

### Container Testing
1. Build Docker image
2. Run container with test environment variables
3. Verify health check endpoint responds
4. Test application functionality

## Support

For issues or questions regarding containerization, please contact the DevOps team.
