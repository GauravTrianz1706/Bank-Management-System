# BankDatabaseAccess - AWS ECS Fargate Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Structure](#project-structure)
4. [Local Development](#local-development)
5. [Docker Containerization](#docker-containerization)
6. [AWS ECS Fargate Deployment](#aws-ecs-fargate-deployment)
7. [Configuration Management](#configuration-management)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Troubleshooting](#troubleshooting)
10. [Security Considerations](#security-considerations)
11. [Scaling and Performance](#scaling-and-performance)

---

## Overview

BankDatabaseAccess is a .NET 8.0 class library that provides database access functionality for the Bank Management System. This guide covers containerization and deployment to AWS ECS Fargate.

### Technology Stack
- **.NET Version**: 8.0
- **Framework**: .NET Runtime
- **Application Type**: Class Library (part of Windows Forms application)
- **Database**: SQL Server (via System.Data.SqlClient)
- **Container Runtime**: Docker
- **Orchestration**: AWS ECS Fargate
- **Health Check**: Custom HTTP endpoint on port 8080

### Key Features
- Multi-stage Docker build for optimized image size
- Built-in health check endpoint at `/health`
- AWS ECS Fargate deployment with auto-scaling support
- CloudWatch integration for logging and monitoring
- Application Load Balancer support for high availability

---

## Prerequisites

### Required Software
1. **Docker Desktop** (version 20.10 or later)
   - Download: https://www.docker.com/products/docker-desktop
   - Verify: `docker --version`

2. **AWS CLI** (version 2.x)
   - Download: https://aws.amazon.com/cli/
   - Verify: `aws --version`
   - Configure: `aws configure`

3. **.NET SDK 8.0** (for local development)
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify: `dotnet --version`

### AWS Account Requirements
1. **IAM User/Role** with permissions for:
   - ECS (Full Access)
   - ECR (Full Access)
   - EC2 (VPC, Security Groups, Subnets)
   - CloudWatch Logs (Full Access)
   - Elastic Load Balancing (Full Access)
   - IAM (Role creation and management)

2. **VPC Configuration**:
   - VPC with at least 2 subnets in different availability zones
   - Internet Gateway attached to VPC
   - Route tables configured for internet access
   - Security groups allowing inbound traffic on port 8080

3. **IAM Roles**:
   - **ecsTaskExecutionRole**: Allows ECS to pull images and write logs
   - **ecsTaskRole**: Allows tasks to access AWS services (optional)

### Creating Required IAM Roles

#### ECS Task Execution Role
```bash
# Create trust policy
cat > ecs-task-execution-trust-policy.json <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ecs-tasks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

# Create role
aws iam create-role \
  --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach managed policy
aws iam attach-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

#### ECS Task Role (Optional)
```bash
# Create role
aws iam create-role \
  --role-name ecsTaskRole \
  --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach policies as needed for your application
# Example: S3 access, DynamoDB access, etc.
```

---

## Project Structure

```
BankDatabaseAccess/
├── Dockerfile                      # Multi-stage Docker build file
├── docker-compose.yml              # Local development orchestration
├── .dockerignore                   # Docker build exclusions
├── BankDatabaseAccess.csproj       # .NET project file
├── scripts/
│   ├── build-push.sh              # Linux/macOS build and push script
│   ├── build-push.bat             # Windows build and push script
│   ├── deploy-image.sh            # Linux/macOS ECS deployment script
│   └── deploy-image.bat           # Windows ECS deployment script
├── ecs/
│   ├── task-definition.json       # ECS Fargate task definition
│   └── service-definition.json    # ECS service configuration
├── docs/
│   └── DEPLOYMENT.md              # This file
└── DatabaseOperation/
    ├── CustomerOperation.cs
    ├── EmployeeOperations.cs
    └── ...
```

---

## Local Development

### Building the Application Locally

```bash
# Navigate to solution directory
cd /path/to/abcc

# Restore dependencies
dotnet restore BankManagementSystem.sln

# Build the solution
dotnet build BankManagementSystem.sln -c Release

# Run tests (if available)
dotnet test BankManagementSystem.sln
```

### Running with Docker Compose

```bash
# Navigate to BankDatabaseAccess directory
cd BankDatabaseAccess

# Build and start the container
docker-compose up --build

# Run in detached mode
docker-compose up -d

# View logs
docker-compose logs -f

# Stop containers
docker-compose down
```

### Testing the Health Endpoint

```bash
# Test health endpoint
curl http://localhost:8080/health

# Expected response:
# {
#   "status": "UP",
#   "timestamp": "2024-01-15T10:30:00Z",
#   "application": "BankManagementSystem",
#   "version": "1.0.0"
# }
```

---

## Docker Containerization

### Understanding the Dockerfile

The Dockerfile uses a multi-stage build approach:

**Stage 1: Builder**
- Base image: `mcr.microsoft.com/dotnet/sdk:8.0`
- Restores NuGet packages
- Builds the solution
- Publishes the application

**Stage 2: Runtime**
- Base image: `mcr.microsoft.com/dotnet/runtime:8.0`
- Copies published artifacts
- Creates non-root user for security
- Exposes port 8080 for health checks
- Sets environment variables

### Building Docker Image Manually

```bash
# From repository root
docker build -f BankDatabaseAccess/Dockerfile -t bankdatabaseaccess:latest .

# Tag for registry
docker tag bankdatabaseaccess:latest <registry>/bankdatabaseaccess:v1.0.0

# Push to registry
docker push <registry>/bankdatabaseaccess:v1.0.0
```

### Using Build Scripts

#### Linux/macOS
```bash
cd BankDatabaseAccess
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Windows
```cmd
cd BankDatabaseAccess
scripts\build-push.bat
```

The scripts will:
1. Prompt for registry type (AWS ECR or Docker Hub)
2. Authenticate with the selected registry
3. Build the Docker image
4. Tag the image appropriately
5. Push to the registry
6. Display the image URI for deployment

---

## AWS ECS Fargate Deployment

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         AWS Cloud                            │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │                    VPC                              │    │
│  │                                                     │    │
│  │  ┌──────────────┐         ┌──────────────┐        │    │
│  │  │  Subnet 1    │         │  Subnet 2    │        │    │
│  │  │  (AZ-1)      │         │  (AZ-2)      │        │    │
│  │  │              │         │              │        │    │
│  │  │  ┌────────┐  │         │  ┌────────┐  │        │    │
│  │  │  │ Task 1 │  │         │  │ Task 2 │  │        │    │
│  │  │  │ :8080  │  │         │  │ :8080  │  │        │    │
│  │  │  └────────┘  │         │  └────────┘  │        │    │
│  │  │      ▲       │         │      ▲       │        │    │
│  │  └──────┼───────┘         └──────┼───────┘        │    │
│  │         │                        │                │    │
│  │         └────────────┬───────────┘                │    │
│  │                      │                            │    │
│  │              ┌───────▼────────┐                   │    │
│  │              │  Load Balancer │                   │    │
│  │              │   (Port 80)    │                   │    │
│  │              └───────▲────────┘                   │    │
│  └──────────────────────┼──────────────────────────┘    │
│                         │                                │
│                    ┌────▼─────┐                          │
│                    │ Internet │                          │
│                    │ Gateway  │                          │
│                    └──────────┘                          │
└─────────────────────────────────────────────────────────────┘
```

### ECS Fargate Prerequisites

1. **VPC Setup**:
   ```bash
   # Create VPC
   aws ec2 create-vpc --cidr-block 10.0.0.0/16
   
   # Create subnets
   aws ec2 create-subnet --vpc-id <vpc-id> --cidr-block 10.0.1.0/24 --availability-zone us-east-1a
   aws ec2 create-subnet --vpc-id <vpc-id> --cidr-block 10.0.2.0/24 --availability-zone us-east-1b
   
   # Create and attach internet gateway
   aws ec2 create-internet-gateway
   aws ec2 attach-internet-gateway --vpc-id <vpc-id> --internet-gateway-id <igw-id>
   ```

2. **Security Group**:
   ```bash
   # Create security group
   aws ec2 create-security-group \
     --group-name bankdatabaseaccess-sg \
     --description "Security group for BankDatabaseAccess" \
     --vpc-id <vpc-id>
   
   # Allow inbound traffic on port 8080
   aws ec2 authorize-security-group-ingress \
     --group-id <sg-id> \
     --protocol tcp \
     --port 8080 \
     --cidr 0.0.0.0/0
   
   # Allow inbound traffic on port 80 (for ALB)
   aws ec2 authorize-security-group-ingress \
     --group-id <sg-id> \
     --protocol tcp \
     --port 80 \
     --cidr 0.0.0.0/0
   ```

3. **CloudWatch Log Group**:
   ```bash
   aws logs create-log-group --log-group-name /ecs/bankdatabaseaccess
   ```

### ECS Task Definition Explained

The task definition (`ecs/task-definition.json`) specifies:

- **Launch Type**: FARGATE (serverless container execution)
- **Network Mode**: awsvpc (each task gets its own ENI)
- **CPU**: 512 (.5 vCPU)
- **Memory**: 1024 MB (1 GB)
- **Execution Role**: Allows ECS to pull images and write logs
- **Task Role**: Allows tasks to access AWS services

**Valid Fargate CPU/Memory Combinations**:
- CPU: 256 → Memory: 512, 1024, 2048 MB
- CPU: 512 → Memory: 1024, 2048, 3072, 4096 MB
- CPU: 1024 → Memory: 2048-8192 MB (increments of 1024)
- CPU: 2048 → Memory: 4096-16384 MB (increments of 1024)
- CPU: 4096 → Memory: 8192-30720 MB (increments of 1024)

### ECS Service Configuration

The service definition (`ecs/service-definition.json`) specifies:

- **Desired Count**: 2 (number of tasks to run)
- **Launch Type**: FARGATE
- **Network Configuration**: Subnets, security groups, public IP
- **Deployment Configuration**: Rolling update strategy
- **Load Balancer**: Optional ALB integration
- **Health Check Grace Period**: 300 seconds (5 minutes)

### Deployment Walkthrough

#### Step 1: Build and Push Docker Image

```bash
# Linux/macOS
cd BankDatabaseAccess
./scripts/build-push.sh

# Windows
cd BankDatabaseAccess
scripts\build-push.bat
```

Follow the prompts to:
1. Select registry (AWS ECR or Docker Hub)
2. Enter registry credentials
3. Specify image tag

**Example Output**:
```
========================================
Docker Build and Push Script
========================================

Project: bankdatabaseaccess

Select Container Registry:
1. AWS ECR (Elastic Container Registry)
2. Docker Hub
Enter choice (1 or 2): 1

=== AWS ECR Configuration ===
Enter AWS Region (e.g., us-east-1): us-east-1
Enter AWS Account ID: 123456789012
Enter ECR Repository Name (default: bankdatabaseaccess): bankdatabaseaccess
Enter image tag (default: latest): v1.0.0

Authenticating with AWS ECR...
ECR authentication successful!

=== Building Docker Image ===
Image: 123456789012.dkr.ecr.us-east-1.amazonaws.com/bankdatabaseaccess:v1.0.0

[Docker build output...]

Docker build successful!

=== Pushing Docker Image ===
[Docker push output...]

========================================
Build and Push Completed Successfully!
========================================

Image: 123456789012.dkr.ecr.us-east-1.amazonaws.com/bankdatabaseaccess:v1.0.0
```

#### Step 2: Deploy to ECS Fargate

```bash
# Linux/macOS
./scripts/deploy-image.sh

# Windows
scripts\deploy-image.bat
```

Follow the prompts to:
1. Enter AWS region
2. Enter ECS cluster name (will be created if doesn't exist)
3. Enter VPC ID
4. Enter subnet IDs (comma-separated)
5. Enter security group ID
6. Enter Docker image URI (from Step 1)
7. Enter database configuration
8. Choose whether to create a load balancer

**Example Output**:
```
========================================
AWS ECS Fargate Deployment Script
========================================

=== AWS Configuration ===
Enter AWS region (e.g., us-east-1): us-east-1
Enter ECS cluster name (e.g., my-ecs-cluster): bankdatabaseaccess-cluster

Retrieving AWS Account ID...
Account ID: 123456789012

Checking ECS cluster...
Cluster ready: bankdatabaseaccess-cluster

=== Network Configuration ===
Enter VPC ID (e.g., vpc-0abc123def456): vpc-0abc123def456
Enter Subnet IDs comma-separated: subnet-0abc123,subnet-0def456
Enter Security Group ID (e.g., sg-0abc123def): sg-0abc123def

=== Docker Image Configuration ===
Enter Docker image URI: 123456789012.dkr.ecr.us-east-1.amazonaws.com/bankdatabaseaccess:v1.0.0

=== Database Configuration ===
Enter database server: mydb.us-east-1.rds.amazonaws.com
Enter database name: BankDB
Enter database user: dbadmin
Enter database password: ********

Do you need a load balancer for this service? (y/n): y

Creating Application Load Balancer...
Load Balancer ARN: arn:aws:elasticloadbalancing:...
Target Group ARN: arn:aws:elasticloadbalancing:...
Load Balancer DNS: bankdatabaseaccess-alb-123456789.us-east-1.elb.amazonaws.com

Creating CloudWatch log group...

Preparing task definition...
Registering task definition...
Task definition registered: arn:aws:ecs:us-east-1:123456789012:task-definition/bankdatabaseaccess-task:1

Creating new ECS service...
Service created successfully!

Waiting for service to stabilize (this may take a few minutes)...

=== Deployment Status ===
┌─────────────────────────────┬────────┬──────────────┬──────────────┐
│ serviceName                 │ status │ runningCount │ desiredCount │
├─────────────────────────────┼────────┼──────────────┼──────────────┤
│ bankdatabaseaccess-service  │ ACTIVE │ 2            │ 2            │
└─────────────────────────────┴────────┴──────────────┴──────────────┘

========================================
Deployment Completed Successfully!
========================================

Service Details:
  Cluster: bankdatabaseaccess-cluster
  Service: bankdatabaseaccess-service
  Region: us-east-1

Application Access:
  Load Balancer: http://bankdatabaseaccess-alb-123456789.us-east-1.elb.amazonaws.com
  Health Check: http://bankdatabaseaccess-alb-123456789.us-east-1.elb.amazonaws.com/health

CloudWatch Logs:
  Log Group: /ecs/bankdatabaseaccess
  Region: us-east-1
```

#### Step 3: Verify Deployment

```bash
# Check service status
aws ecs describe-services \
  --cluster bankdatabaseaccess-cluster \
  --services bankdatabaseaccess-service \
  --region us-east-1

# List running tasks
aws ecs list-tasks \
  --cluster bankdatabaseaccess-cluster \
  --service-name bankdatabaseaccess-service \
  --region us-east-1

# Test health endpoint
curl http://<load-balancer-dns>/health
```

---

## Configuration Management

### Environment Variables

The application uses the following environment variables:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `DOTNET_ENVIRONMENT` | .NET environment | Production | Yes |
| `HEALTH_CHECK_PORT` | Health check endpoint port | 8080 | Yes |
| `CURRENT_USER` | Application user context | containeruser | Yes |
| `TZ` | Timezone | UTC | No |
| `DB_CONNECTION_STRING` | Full database connection string | - | Yes |
| `DB_SERVER` | Database server hostname | - | Yes |
| `DB_NAME` | Database name | - | Yes |
| `DB_USER` | Database username | - | Yes |
| `DB_PASSWORD` | Database password | - | Yes |

### Updating Configuration

#### Update Task Definition
```bash
# Edit ecs/task-definition.json
# Update environment variables in containerDefinitions[0].environment

# Register new task definition
aws ecs register-task-definition \
  --cli-input-json file://ecs/task-definition.json \
  --region us-east-1

# Update service with new task definition
aws ecs update-service \
  --cluster bankdatabaseaccess-cluster \
  --service bankdatabaseaccess-service \
  --task-definition bankdatabaseaccess-task:2 \
  --region us-east-1
```

#### Using AWS Secrets Manager (Recommended)
```bash
# Store database password in Secrets Manager
aws secretsmanager create-secret \
  --name bankdatabaseaccess/db-password \
  --secret-string "your-secure-password" \
  --region us-east-1

# Update task definition to use secrets
# Add to containerDefinitions[0]:
"secrets": [
  {
    "name": "DB_PASSWORD",
    "valueFrom": "arn:aws:secretsmanager:us-east-1:123456789012:secret:bankdatabaseaccess/db-password"
  }
]
```

---

## Monitoring and Logging

### CloudWatch Logs

All application logs are sent to CloudWatch Logs:

**Log Group**: `/ecs/bankdatabaseaccess`
**Log Stream**: `ecs/<task-id>`

#### Viewing Logs
```bash
# Tail logs in real-time
aws logs tail /ecs/bankdatabaseaccess --follow --region us-east-1

# View logs for specific time range
aws logs filter-log-events \
  --log-group-name /ecs/bankdatabaseaccess \
  --start-time $(date -d '1 hour ago' +%s)000 \
  --region us-east-1

# Search logs
aws logs filter-log-events \
  --log-group-name /ecs/bankdatabaseaccess \
  --filter-pattern "ERROR" \
  --region us-east-1
```

### CloudWatch Metrics

ECS automatically publishes metrics to CloudWatch:

- **CPUUtilization**: Task CPU usage
- **MemoryUtilization**: Task memory usage
- **TargetResponseTime**: ALB response time
- **HealthyHostCount**: Number of healthy targets
- **UnHealthyHostCount**: Number of unhealthy targets

#### Creating CloudWatch Dashboard
```bash
# Create dashboard JSON
cat > dashboard.json <<EOF
{
  "widgets": [
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ECS", "CPUUtilization", {"stat": "Average"}],
          [".", "MemoryUtilization", {"stat": "Average"}]
        ],
        "period": 300,
        "stat": "Average",
        "region": "us-east-1",
        "title": "ECS Task Metrics"
      }
    }
  ]
}
EOF

# Create dashboard
aws cloudwatch put-dashboard \
  --dashboard-name BankDatabaseAccess \
  --dashboard-body file://dashboard.json \
  --region us-east-1
```

### Health Checks

The application provides a health check endpoint at `/health`:

**Endpoint**: `http://<task-ip>:8080/health`

**Response**:
```json
{
  "status": "UP",
  "timestamp": "2024-01-15T10:30:00Z",
  "application": "BankManagementSystem",
  "version": "1.0.0"
}
```

**ALB Health Check Configuration**:
- Protocol: HTTP
- Path: `/health`
- Port: 8080
- Interval: 30 seconds
- Timeout: 5 seconds
- Healthy threshold: 2
- Unhealthy threshold: 3

---

## Troubleshooting

### Common Issues and Solutions

#### 1. Task Fails to Start

**Symptoms**:
- Tasks transition to STOPPED state immediately
- Error: "CannotPullContainerError"

**Solutions**:
```bash
# Check task stopped reason
aws ecs describe-tasks \
  --cluster bankdatabaseaccess-cluster \
  --tasks <task-id> \
  --region us-east-1 \
  --query 'tasks[0].stoppedReason'

# Verify ECR permissions
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Check execution role permissions
aws iam get-role --role-name ecsTaskExecutionRole
```

#### 2. Health Check Failures

**Symptoms**:
- Tasks marked as unhealthy
- Service keeps replacing tasks

**Solutions**:
```bash
# Check task logs
aws logs tail /ecs/bankdatabaseaccess --follow --region us-east-1

# Test health endpoint directly
aws ecs describe-tasks \
  --cluster bankdatabaseaccess-cluster \
  --tasks <task-id> \
  --region us-east-1 \
  --query 'tasks[0].attachments[0].details[?name==`privateIPv4Address`].value' \
  --output text

curl http://<task-ip>:8080/health

# Increase health check grace period
aws ecs update-service \
  --cluster bankdatabaseaccess-cluster \
  --service bankdatabaseaccess-service \
  --health-check-grace-period-seconds 600 \
  --region us-east-1
```

#### 3. Database Connection Issues

**Symptoms**:
- Application logs show connection errors
- Tasks fail to start or crash

**Solutions**:
```bash
# Verify security group allows database access
aws ec2 describe-security-groups --group-ids <sg-id> --region us-east-1

# Test database connectivity from task
aws ecs execute-command \
  --cluster bankdatabaseaccess-cluster \
  --task <task-id> \
  --container bankdatabaseaccess \
  --interactive \
  --command "/bin/bash"

# Inside container:
# apt-get update && apt-get install -y telnet
# telnet <db-server> 1433

# Check connection string format
# Verify DB_SERVER, DB_NAME, DB_USER, DB_PASSWORD environment variables
```

#### 4. Out of Memory Errors

**Symptoms**:
- Tasks stop with "OutOfMemoryError"
- High memory utilization in CloudWatch

**Solutions**:
```bash
# Increase task memory
# Edit ecs/task-definition.json
# Change "memory": "1024" to "memory": "2048"

# Register new task definition
aws ecs register-task-definition \
  --cli-input-json file://ecs/task-definition.json \
  --region us-east-1

# Update service
aws ecs update-service \
  --cluster bankdatabaseaccess-cluster \
  --service bankdatabaseaccess-service \
  --task-definition bankdatabaseaccess-task:2 \
  --force-new-deployment \
  --region us-east-1
```

#### 5. Load Balancer 502/503 Errors

**Symptoms**:
- ALB returns 502 Bad Gateway or 503 Service Unavailable
- Health checks failing

**Solutions**:
```bash
# Check target health
aws elbv2 describe-target-health \
  --target-group-arn <target-group-arn> \
  --region us-east-1

# Verify security group allows ALB to reach tasks
# Security group must allow inbound traffic from ALB security group on port 8080

# Check ALB access logs
aws elbv2 modify-load-balancer-attributes \
  --load-balancer-arn <alb-arn> \
  --attributes Key=access_logs.s3.enabled,Value=true Key=access_logs.s3.bucket,Value=<bucket-name> \
  --region us-east-1
```

### Debugging Commands

```bash
# View service events
aws ecs describe-services \
  --cluster bankdatabaseaccess-cluster \
  --services bankdatabaseaccess-service \
  --region us-east-1 \
  --query 'services[0].events[0:10]'

# Get task details
aws ecs describe-tasks \
  --cluster bankdatabaseaccess-cluster \
  --tasks <task-id> \
  --region us-east-1

# Check task definition
aws ecs describe-task-definition \
  --task-definition bankdatabaseaccess-task \
  --region us-east-1

# View container logs
aws logs get-log-events \
  --log-group-name /ecs/bankdatabaseaccess \
  --log-stream-name ecs/<task-id>/bankdatabaseaccess/<container-id> \
  --region us-east-1
```

---

## Security Considerations

### Container Security

1. **Non-Root User**:
   - Application runs as `appuser` (non-root)
   - Reduces attack surface

2. **Minimal Base Image**:
   - Uses official Microsoft runtime image
   - No unnecessary packages installed

3. **Read-Only Root Filesystem** (Optional):
   ```json
   "containerDefinitions": [{
     "readonlyRootFilesystem": true,
     "mountPoints": [
       {
         "sourceVolume": "tmp",
         "containerPath": "/tmp",
         "readOnly": false
       }
     ]
   }]
   ```

### Network Security

1. **Security Groups**:
   - Restrict inbound traffic to necessary ports only
   - Use separate security groups for ALB and tasks
   - Allow outbound traffic only to required services

2. **Private Subnets** (Recommended):
   - Deploy tasks in private subnets
   - Use NAT Gateway for outbound internet access
   - ALB in public subnets

3. **VPC Endpoints**:
   - Use VPC endpoints for AWS services (ECR, CloudWatch, Secrets Manager)
   - Reduces data transfer costs
   - Improves security

### Secrets Management

1. **AWS Secrets Manager**:
   ```bash
   # Store database credentials
   aws secretsmanager create-secret \
     --name bankdatabaseaccess/db-credentials \
     --secret-string '{"username":"dbadmin","password":"secure-password"}' \
     --region us-east-1
   
   # Reference in task definition
   "secrets": [
     {
       "name": "DB_USER",
       "valueFrom": "arn:aws:secretsmanager:region:account:secret:bankdatabaseaccess/db-credentials:username::"
     },
     {
       "name": "DB_PASSWORD",
       "valueFrom": "arn:aws:secretsmanager:region:account:secret:bankdatabaseaccess/db-credentials:password::"
     }
   ]
   ```

2. **IAM Policies**:
   - Grant task role minimum required permissions
   - Use resource-based policies where possible

### Compliance

1. **Encryption**:
   - Enable encryption at rest for CloudWatch Logs
   - Use SSL/TLS for database connections
   - Enable ALB HTTPS listener with ACM certificate

2. **Audit Logging**:
   - Enable CloudTrail for API calls
   - Enable VPC Flow Logs
   - Enable ALB access logs

---

## Scaling and Performance

### Auto Scaling

#### Target Tracking Scaling
```bash
# Create scaling policy based on CPU utilization
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/bankdatabaseaccess-cluster/bankdatabaseaccess-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10 \
  --region us-east-1

aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/bankdatabaseaccess-cluster/bankdatabaseaccess-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name cpu-target-tracking \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json \
  --region us-east-1
```

**scaling-policy.json**:
```json
{
  "TargetValue": 70.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
  },
  "ScaleInCooldown": 300,
  "ScaleOutCooldown": 60
}
```

#### Step Scaling
```bash
# Create CloudWatch alarm
aws cloudwatch put-metric-alarm \
  --alarm-name bankdatabaseaccess-high-cpu \
  --alarm-description "Trigger scaling when CPU > 80%" \
  --metric-name CPUUtilization \
  --namespace AWS/ECS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --region us-east-1

# Create scaling policy
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/bankdatabaseaccess-cluster/bankdatabaseaccess-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name step-scaling-policy \
  --policy-type StepScaling \
  --step-scaling-policy-configuration file://step-scaling-policy.json \
  --region us-east-1
```

### Performance Optimization

1. **.NET Runtime Optimization**:
   - Use ReadyToRun (R2R) compilation for faster startup
   - Enable tiered compilation
   - Configure garbage collection for containerized environments

   ```dockerfile
   # Add to Dockerfile
   ENV DOTNET_ReadyToRun=1 \
       DOTNET_TieredCompilation=1 \
       DOTNET_gcServer=1
   ```

2. **Connection Pooling**:
   - Configure SQL Server connection pooling
   - Set appropriate pool size limits
   - Monitor connection usage

3. **Caching**:
   - Implement application-level caching
   - Use Redis or ElastiCache for distributed caching
   - Cache frequently accessed data

4. **Database Optimization**:
   - Use read replicas for read-heavy workloads
   - Implement connection pooling
   - Optimize queries and indexes

### Blue/Green Deployments

```bash
# Create new task definition version
aws ecs register-task-definition \
  --cli-input-json file://ecs/task-definition-v2.json \
  --region us-east-1

# Update service with deployment configuration
aws ecs update-service \
  --cluster bankdatabaseaccess-cluster \
  --service bankdatabaseaccess-service \
  --task-definition bankdatabaseaccess-task:2 \
  --deployment-configuration "maximumPercent=200,minimumHealthyPercent=100" \
  --region us-east-1

# Monitor deployment
aws ecs describe-services \
  --cluster bankdatabaseaccess-cluster \
  --services bankdatabaseaccess-service \
  --region us-east-1 \
  --query 'services[0].deployments'
```

### Cost Optimization

1. **Right-Sizing**:
   - Monitor CPU and memory utilization
   - Adjust task CPU/memory based on actual usage
   - Use Fargate Spot for non-critical workloads

2. **Fargate Spot**:
   ```json
   "capacityProviderStrategy": [
     {
       "capacityProvider": "FARGATE_SPOT",
       "weight": 1,
       "base": 0
     },
     {
       "capacityProvider": "FARGATE",
       "weight": 1,
       "base": 2
     }
   ]
   ```

3. **Scheduled Scaling**:
   - Scale down during off-peak hours
   - Scale up before peak traffic periods

---

## Additional Resources

### AWS Documentation
- [ECS Fargate Documentation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)
- [ECS Task Definitions](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_definitions.html)
- [ECS Service Auto Scaling](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/service-auto-scaling.html)
- [CloudWatch Logs](https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/WhatIsCloudWatchLogs.html)

### .NET Documentation
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Containerize .NET Applications](https://docs.microsoft.com/en-us/dotnet/core/docker/introduction)
- [.NET Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/core/performance/)

### Docker Documentation
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Multi-Stage Builds](https://docs.docker.com/develop/develop-images/multistage-build/)
- [Dockerfile Reference](https://docs.docker.com/engine/reference/builder/)

---

## Support and Maintenance

### Updating the Application

1. Make code changes
2. Build and push new Docker image
3. Register new task definition
4. Update ECS service
5. Monitor deployment

### Rollback Procedure

```bash
# List task definition revisions
aws ecs list-task-definitions \
  --family-prefix bankdatabaseaccess-task \
  --region us-east-1

# Rollback to previous version
aws ecs update-service \
  --cluster bankdatabaseaccess-cluster \
  --service bankdatabaseaccess-service \
  --task-definition bankdatabaseaccess-task:1 \
  --region us-east-1
```

### Backup and Disaster Recovery

1. **Database Backups**:
   - Enable automated RDS backups
   - Configure backup retention period
   - Test restore procedures

2. **Configuration Backups**:
   - Version control all configuration files
   - Store task definitions in Git
   - Document infrastructure as code

3. **Disaster Recovery Plan**:
   - Multi-region deployment strategy
   - Regular DR drills
   - Documented recovery procedures

---

## Conclusion

This deployment guide provides comprehensive instructions for containerizing and deploying the BankDatabaseAccess application to AWS ECS Fargate. Follow the steps carefully, and refer to the troubleshooting section for common issues.

For additional support or questions, please contact the development team or refer to the AWS documentation.

**Version**: 1.0.0  
**Last Updated**: 2024-01-15  
**Maintained By**: DevOps Team
