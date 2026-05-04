# BankManagementSystem - AWS ECS Fargate Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Architecture](#project-architecture)
4. [Local Development Setup](#local-development-setup)
5. [Docker Containerization](#docker-containerization)
6. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
7. [ECS Fargate Setup](#ecs-fargate-setup)
8. [Deployment Process](#deployment-process)
9. [Configuration Management](#configuration-management)
10. [Monitoring and Logging](#monitoring-and-logging)
11. [Troubleshooting](#troubleshooting)
12. [Security Considerations](#security-considerations)
13. [Scaling and Performance](#scaling-and-performance)

---

## Overview

BankManagementSystem is a .NET 6.0 Windows Forms application that has been containerized for deployment on AWS ECS Fargate. This guide provides comprehensive instructions for building, deploying, and managing the application in a containerized environment.

### Technology Stack
- **.NET Version**: 6.0
- **Application Type**: Windows Forms Application with HTTP Health Check Endpoint
- **Container Runtime**: Docker
- **Deployment Platform**: AWS ECS Fargate
- **Orchestration**: AWS ECS
- **Logging**: AWS CloudWatch Logs
- **Load Balancing**: AWS Application Load Balancer (Optional)

### Key Features
- Multi-stage Docker build for optimized image size
- Health check endpoint on port 8080
- CloudWatch integration for centralized logging
- Fargate deployment for serverless container management
- Auto-scaling capabilities
- High availability with multiple availability zones

---

## Prerequisites

### Required Tools and Software

#### 1. Docker
- **Version**: Docker 20.10 or later
- **Installation**: 
  - Linux: `curl -fsSL https://get.docker.com | sh`
  - macOS: Download Docker Desktop from https://www.docker.com/products/docker-desktop
  - Windows: Download Docker Desktop from https://www.docker.com/products/docker-desktop

#### 2. AWS CLI
- **Version**: AWS CLI v2
- **Installation**:
  ```bash
  # Linux
  curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
  unzip awscliv2.zip
  sudo ./aws/install
  
  # macOS
  curl "https://awscli.amazonaws.com/AWSCLIV2.pkg" -o "AWSCLIV2.pkg"
  sudo installer -pkg AWSCLIV2.pkg -target /
  
  # Windows
  # Download and run: https://awscli.amazonaws.com/AWSCLIV2.msi
  ```

- **Configuration**:
  ```bash
  aws configure
  # Enter your AWS Access Key ID
  # Enter your AWS Secret Access Key
  # Enter default region (e.g., us-east-1)
  # Enter default output format (json)
  ```

#### 3. .NET SDK (for local development)
- **Version**: .NET 6.0 SDK
- **Installation**: Download from https://dotnet.microsoft.com/download/dotnet/6.0

#### 4. Git
- **Version**: Git 2.x or later
- **Installation**: https://git-scm.com/downloads

### AWS Account Requirements

#### 1. IAM Permissions
Your AWS user/role must have permissions for:
- ECS (create/update clusters, services, task definitions)
- ECR (create repositories, push/pull images)
- EC2 (VPC, subnets, security groups)
- IAM (create/manage ECS task execution roles)
- CloudWatch Logs (create log groups, write logs)
- Elastic Load Balancing (create/manage ALBs and target groups)

#### 2. IAM Roles Required

**ECS Task Execution Role** (`ecsTaskExecutionRole`):
```json
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
```

Attach managed policy: `arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy`

**ECS Task Role** (`ecsTaskRole`) - Optional, for application-level AWS access:
```json
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
```

Create these roles using AWS CLI:
```bash
# Create execution role
aws iam create-role --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://ecs-task-execution-role-trust-policy.json

aws iam attach-role-policy --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy

# Create task role
aws iam create-role --role-name ecsTaskRole \
  --assume-role-policy-document file://ecs-task-role-trust-policy.json
```

---

## Project Architecture

### Application Structure
```
BankManagementSystem/
├── BankManagementSystem/          # Main application project
│   ├── Program.cs                 # Application entry point
│   ├── HealthCheckEndpoint.cs     # HTTP health check endpoint
│   ├── BankManagementSystem.csproj
│   └── ...                        # Other application files
├── BankDatabaseAccess/            # Database access layer
│   └── BankDatabaseAccess.csproj
├── Dockerfile                     # Multi-stage Docker build
├── docker-compose.yml             # Local development orchestration
├── .dockerignore                  # Docker build exclusions
├── scripts/
│   ├── build-push.sh              # Build and push script (Linux/macOS)
│   ├── build-push.bat             # Build and push script (Windows)
│   ├── deploy-image.sh            # ECS deployment script (Linux/macOS)
│   └── deploy-image.bat           # ECS deployment script (Windows)
├── ecs/
│   ├── task-definition.json       # ECS task definition
│   └── service-definition.json    # ECS service definition
└── docs/
    └── DEPLOYMENT.md              # This file
```

### Container Architecture

#### Multi-Stage Build
The Dockerfile uses a multi-stage build approach:

1. **Builder Stage** (`mcr.microsoft.com/dotnet/sdk:6.0`):
   - Restores NuGet packages
   - Compiles the application
   - Publishes release artifacts

2. **Runtime Stage** (`mcr.microsoft.com/dotnet/runtime:6.0`):
   - Minimal runtime image
   - Non-root user for security
   - Only published artifacts (no source code)

#### Health Check Endpoint
The application includes a custom HTTP health check endpoint:
- **Port**: 8080 (configurable via `HEALTH_CHECK_PORT` environment variable)
- **Path**: `/health/`
- **Response**: JSON with status information
- **Purpose**: Used by ECS and ALB for health monitoring

---

## Local Development Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd BankManagementSystem
```

### 2. Build the Application Locally
```bash
# Restore dependencies
dotnet restore BankManagementSystem/BankManagementSystem.csproj

# Build the application
dotnet build BankManagementSystem/BankManagementSystem.csproj -c Release

# Run the application (requires interactive session)
dotnet run --project BankManagementSystem/BankManagementSystem.csproj
```

### 3. Build Docker Image Locally
```bash
# Build the Docker image
docker build -t bankmanagementsystem:local .

# Verify the image
docker images | grep bankmanagementsystem
```

### 4. Run Container Locally
```bash
# Run using docker-compose
docker-compose up -d

# Check container status
docker-compose ps

# View logs
docker-compose logs -f

# Test health endpoint
curl http://localhost:8080/health/

# Stop containers
docker-compose down
```

---

## Docker Containerization

### Dockerfile Explanation

#### Stage 1: Build
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /src

# Copy project files for dependency caching
COPY BankDatabaseAccess/BankDatabaseAccess.csproj BankDatabaseAccess/
COPY BankManagementSystem/BankManagementSystem.csproj BankManagementSystem/

# Restore dependencies (cached layer)
RUN dotnet restore BankManagementSystem/BankManagementSystem.csproj

# Copy source code
COPY BankDatabaseAccess/ BankDatabaseAccess/
COPY BankManagementSystem/ BankManagementSystem/

# Build and publish
WORKDIR /src/BankManagementSystem
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /app/publish --no-build
```

#### Stage 2: Runtime
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:6.0

WORKDIR /app

# Create non-root user
RUN groupadd -r bankapp && useradd -r -g bankapp bankapp

# Copy published artifacts
COPY --from=builder /app/publish .

# Set ownership
RUN chown -R bankapp:bankapp /app

# Switch to non-root user
USER bankapp

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    HEALTH_CHECK_PORT=8080 \
    STARTUP_USER=containerized

# Expose port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "BankManagementSystem.dll"]
```

### Build Optimization Tips

1. **Layer Caching**: Project files are copied before source code to leverage Docker layer caching
2. **Multi-Stage Build**: Reduces final image size by excluding build tools
3. **Non-Root User**: Enhances security by running as unprivileged user
4. **.dockerignore**: Excludes unnecessary files from build context

---

## AWS ECS Fargate Prerequisites

### 1. VPC and Network Setup

#### Create VPC (if not exists)
```bash
# Create VPC
VPC_ID=$(aws ec2 create-vpc --cidr-block 10.0.0.0/16 \
  --query 'Vpc.VpcId' --output text)

# Enable DNS hostnames
aws ec2 modify-vpc-attribute --vpc-id $VPC_ID --enable-dns-hostnames

# Create Internet Gateway
IGW_ID=$(aws ec2 create-internet-gateway \
  --query 'InternetGateway.InternetGatewayId' --output text)

# Attach Internet Gateway to VPC
aws ec2 attach-internet-gateway --vpc-id $VPC_ID --internet-gateway-id $IGW_ID
```

#### Create Subnets
```bash
# Create public subnet in AZ 1
SUBNET_1=$(aws ec2 create-subnet --vpc-id $VPC_ID \
  --cidr-block 10.0.1.0/24 --availability-zone us-east-1a \
  --query 'Subnet.SubnetId' --output text)

# Create public subnet in AZ 2
SUBNET_2=$(aws ec2 create-subnet --vpc-id $VPC_ID \
  --cidr-block 10.0.2.0/24 --availability-zone us-east-1b \
  --query 'Subnet.SubnetId' --output text)

# Enable auto-assign public IP
aws ec2 modify-subnet-attribute --subnet-id $SUBNET_1 --map-public-ip-on-launch
aws ec2 modify-subnet-attribute --subnet-id $SUBNET_2 --map-public-ip-on-launch

# Create route table
ROUTE_TABLE=$(aws ec2 create-route-table --vpc-id $VPC_ID \
  --query 'RouteTable.RouteTableId' --output text)

# Add route to Internet Gateway
aws ec2 create-route --route-table-id $ROUTE_TABLE \
  --destination-cidr-block 0.0.0.0/0 --gateway-id $IGW_ID

# Associate subnets with route table
aws ec2 associate-route-table --subnet-id $SUBNET_1 --route-table-id $ROUTE_TABLE
aws ec2 associate-route-table --subnet-id $SUBNET_2 --route-table-id $ROUTE_TABLE
```

#### Create Security Group
```bash
# Create security group
SG_ID=$(aws ec2 create-security-group --group-name bankmanagementsystem-sg \
  --description "Security group for BankManagementSystem" \
  --vpc-id $VPC_ID --query 'GroupId' --output text)

# Allow inbound HTTP (port 80) from anywhere
aws ec2 authorize-security-group-ingress --group-id $SG_ID \
  --protocol tcp --port 80 --cidr 0.0.0.0/0

# Allow inbound health check (port 8080) from VPC
aws ec2 authorize-security-group-ingress --group-id $SG_ID \
  --protocol tcp --port 8080 --cidr 10.0.0.0/16

# Allow outbound all traffic
aws ec2 authorize-security-group-egress --group-id $SG_ID \
  --protocol -1 --cidr 0.0.0.0/0
```

### 2. ECR Repository Setup

```bash
# Create ECR repository
aws ecr create-repository --repository-name bankmanagementsystem \
  --region us-east-1

# Get repository URI
REPO_URI=$(aws ecr describe-repositories --repository-names bankmanagementsystem \
  --query 'repositories[0].repositoryUri' --output text)

echo "ECR Repository URI: $REPO_URI"
```

### 3. CloudWatch Log Group

```bash
# Create log group
aws logs create-log-group --log-group-name /ecs/bankmanagementsystem \
  --region us-east-1

# Set retention policy (optional, 7 days)
aws logs put-retention-policy --log-group-name /ecs/bankmanagementsystem \
  --retention-in-days 7
```

---

## ECS Fargate Setup

### Understanding ECS Fargate

AWS Fargate is a serverless compute engine for containers that:
- Eliminates the need to manage EC2 instances
- Automatically scales compute resources
- Charges only for resources used
- Provides built-in security and isolation

### ECS Task Definition Explained

The task definition (`ecs/task-definition.json`) defines:

#### Launch Type Configuration
```json
{
  "requiresCompatibilities": ["FARGATE"],
  "networkMode": "awsvpc"
}
```
- **FARGATE**: Specifies serverless Fargate launch type
- **awsvpc**: Required network mode for Fargate (each task gets its own ENI)

#### CPU and Memory
```json
{
  "cpu": "512",
  "memory": "1024"
}
```

**Valid Fargate CPU/Memory Combinations**:
| CPU (vCPU) | Memory (MB) |
|------------|-------------|
| 256 (.25)  | 512, 1024, 2048 |
| 512 (.5)   | 1024, 2048, 3072, 4096 |
| 1024 (1)   | 2048-8192 (increments of 1024) |
| 2048 (2)   | 4096-16384 (increments of 1024) |
| 4096 (4)   | 8192-30720 (increments of 1024) |

#### Execution Role
```json
{
  "executionRoleArn": "arn:aws:iam::{{ACCOUNT_ID}}:role/ecsTaskExecutionRole"
}
```
- Allows ECS to pull images from ECR
- Enables CloudWatch Logs integration
- Required for Fargate tasks

#### Container Definition
```json
{
  "containerDefinitions": [
    {
      "name": "bankmanagementsystem",
      "image": "{{IMAGE_URI}}",
      "essential": true,
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [...],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/bankmanagementsystem",
          "awslogs-region": "{{AWS_REGION}}",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

### ECS Service Configuration

The service definition (`ecs/service-definition.json`) defines:

#### Service Settings
```json
{
  "serviceName": "bankmanagementsystem-service",
  "desiredCount": 2,
  "launchType": "FARGATE"
}
```
- **desiredCount**: Number of task instances to run (2 for high availability)
- **launchType**: FARGATE for serverless deployment

#### Network Configuration
```json
{
  "networkConfiguration": {
    "awsvpcConfiguration": {
      "subnets": ["subnet-xxx", "subnet-yyy"],
      "securityGroups": ["sg-xxx"],
      "assignPublicIp": "ENABLED"
    }
  }
}
```
- **subnets**: Multiple subnets across AZs for high availability
- **securityGroups**: Controls inbound/outbound traffic
- **assignPublicIp**: ENABLED for internet access (required for pulling images)

#### Deployment Configuration
```json
{
  "deploymentConfiguration": {
    "maximumPercent": 200,
    "minimumHealthyPercent": 50
  }
}
```
- **maximumPercent**: Maximum tasks during deployment (200% = rolling update)
- **minimumHealthyPercent**: Minimum healthy tasks during deployment (50%)

#### Load Balancer Integration (Optional)
```json
{
  "loadBalancers": [
    {
      "targetGroupArn": "{{TARGET_GROUP_ARN}}",
      "containerName": "bankmanagementsystem",
      "containerPort": 8080
    }
  ],
  "healthCheckGracePeriodSeconds": 300
}
```

---

## Deployment Process

### Step 1: Build and Push Docker Image

#### Using Linux/macOS
```bash
cd BankManagementSystem
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Using Windows
```cmd
cd BankManagementSystem
scripts\build-push.bat
```

#### Script Workflow
1. Prompts for registry selection (ECR or Docker Hub)
2. Collects registry credentials and configuration
3. Authenticates with selected registry
4. Creates ECR repository if it doesn't exist (ECR only)
5. Builds Docker image with proper tagging
6. Pushes image to registry
7. Displays image URI for deployment

**Example Output**:
```
==========================================
Docker Build and Push Script
==========================================

Select Docker Registry:
1. AWS ECR (Elastic Container Registry)
2. Docker Hub
Enter your choice (1 or 2): 1

=== AWS ECR Configuration ===
Enter AWS Region (e.g., us-east-1): us-east-1
Enter AWS Account ID: 123456789012
Enter ECR Repository Name (e.g., bankmanagementsystem): bankmanagementsystem
Enter Image Tag (default: latest): v1.0.0

Authenticating with AWS ECR...
Login Succeeded

Checking if ECR repository exists...
Repository exists

==========================================
Building Docker Image
==========================================
Image: 123456789012.dkr.ecr.us-east-1.amazonaws.com/bankmanagementsystem:v1.0.0

[Docker build output...]

==========================================
Pushing Docker Image
==========================================

[Docker push output...]

==========================================
Build and Push Completed Successfully!
==========================================
Image: 123456789012.dkr.ecr.us-east-1.amazonaws.com/bankmanagementsystem:v1.0.0

Next Steps:
1. Update ECS task definition with this image URI
2. Run deploy-image.sh to deploy to AWS ECS
```

### Step 2: Deploy to AWS ECS Fargate

#### Using Linux/macOS
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Using Windows
```cmd
scripts\deploy-image.bat
```

#### Deployment Script Workflow
1. **AWS Configuration**: Prompts for region and cluster name
2. **Network Configuration**: Collects VPC, subnets, and security group IDs
3. **Image Configuration**: Prompts for Docker image URI
4. **Cluster Check**: Creates ECS cluster if it doesn't exist
5. **Load Balancer Setup** (Optional):
   - Creates Application Load Balancer
   - Creates Target Group with health checks
   - Configures listener rules
6. **CloudWatch Setup**: Creates log group for application logs
7. **Task Definition**: Registers task definition with placeholders replaced
8. **Service Deployment**:
   - Creates new service if doesn't exist
   - Updates existing service with new task definition
9. **Stability Wait**: Waits for service to reach stable state
10. **Verification**: Displays service status and access information

**Example Deployment**:
```
==========================================
AWS ECS Fargate Deployment Script
==========================================

=== AWS Configuration ===
Enter AWS Region (e.g., us-east-1): us-east-1
Enter ECS Cluster Name (e.g., my-ecs-cluster): production-cluster

=== Network Configuration ===
Enter VPC ID (e.g., vpc-0abc123def456): vpc-0abc123def456
Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): subnet-0abc123,subnet-0def456
Enter Security Group ID (e.g., sg-0abc123def): sg-0abc123def

=== Docker Image Configuration ===
Enter Docker Image URI: 123456789012.dkr.ecr.us-east-1.amazonaws.com/bankmanagementsystem:v1.0.0

Retrieving AWS Account ID...
AWS Account ID: 123456789012

Checking ECS cluster...
Cluster exists

=== Load Balancer Configuration ===
Do you need a load balancer for this service? (y/n): y

Creating Application Load Balancer and Target Group...
Target Group ARN: arn:aws:elasticloadbalancing:us-east-1:123456789012:targetgroup/bankmanagementsystem-tg/abc123

Creating CloudWatch log group...
Log group created

Preparing task definition...
Registering task definition...
Task Definition ARN: arn:aws:ecs:us-east-1:123456789012:task-definition/bankmanagementsystem-task:1

Preparing service definition...
Creating new ECS service...

Waiting for service to become stable...
[This may take several minutes...]

==========================================
Deployment Completed Successfully!
==========================================

Service Details:
---------------------------------------------------------
|                    DescribeServices                   |
+---------------------------+---------------------------+
|  bankmanagementsystem-service  |  ACTIVE  |  2  |  2  |
+---------------------------+---------------------------+

CloudWatch Logs: /ecs/bankmanagementsystem
Region: us-east-1

Application URL: http://bankmanagementsystem-alb-123456789.us-east-1.elb.amazonaws.com

To view logs:
  aws logs tail /ecs/bankmanagementsystem --follow --region us-east-1
```

### Step 3: Verify Deployment

#### Check Service Status
```bash
aws ecs describe-services \
  --cluster production-cluster \
  --services bankmanagementsystem-service \
  --region us-east-1
```

#### Check Running Tasks
```bash
aws ecs list-tasks \
  --cluster production-cluster \
  --service-name bankmanagementsystem-service \
  --region us-east-1
```

#### Test Health Endpoint
```bash
# If using load balancer
curl http://<alb-dns-name>/health/

# If using public IP (get from task details)
curl http://<task-public-ip>:8080/health/
```

Expected response:
```json
{
  "status": "healthy",
  "service": "BankManagementSystem"
}
```

---

## Configuration Management

### Environment Variables

The application uses environment variables for configuration:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Production | Application environment |
| `HEALTH_CHECK_PORT` | 8080 | Port for health check endpoint |
| `STARTUP_USER` | containerized | User context identifier |
| `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` | false | Globalization settings |
| `DB_CONNECTION_STRING` | - | Database connection string |

### Updating Environment Variables

#### Method 1: Update Task Definition
1. Edit `ecs/task-definition.json`
2. Modify the `environment` section
3. Re-run deployment script

#### Method 2: AWS Console
1. Navigate to ECS → Task Definitions
2. Select task definition family
3. Create new revision
4. Update environment variables
5. Update service to use new revision

#### Method 3: AWS CLI
```bash
# Get current task definition
aws ecs describe-task-definition \
  --task-definition bankmanagementsystem-task \
  --query 'taskDefinition' > task-def.json

# Edit task-def.json to update environment variables

# Register new revision
aws ecs register-task-definition --cli-input-json file://task-def.json

# Update service
aws ecs update-service \
  --cluster production-cluster \
  --service bankmanagementsystem-service \
  --task-definition bankmanagementsystem-task
```

### Secrets Management

For sensitive data (database passwords, API keys), use AWS Secrets Manager:

#### Create Secret
```bash
aws secretsmanager create-secret \
  --name bankmanagementsystem/db-password \
  --secret-string "MySecurePassword123"
```

#### Reference in Task Definition
```json
{
  "secrets": [
    {
      "name": "DB_PASSWORD",
      "valueFrom": "arn:aws:secretsmanager:us-east-1:123456789012:secret:bankmanagementsystem/db-password"
    }
  ]
}
```

---

## Monitoring and Logging

### CloudWatch Logs

#### View Logs in Real-Time
```bash
# Tail logs
aws logs tail /ecs/bankmanagementsystem --follow --region us-east-1

# Filter logs by pattern
aws logs tail /ecs/bankmanagementsystem --follow \
  --filter-pattern "ERROR" --region us-east-1

# View logs for specific time range
aws logs tail /ecs/bankmanagementsystem \
  --since 1h --region us-east-1
```

#### CloudWatch Logs Insights
Navigate to CloudWatch → Logs Insights and run queries:

```sql
# Find all errors
fields @timestamp, @message
| filter @message like /ERROR/
| sort @timestamp desc
| limit 100

# Count requests by status
fields @timestamp, @message
| stats count() by status
| sort count desc

# Average response time
fields @timestamp, responseTime
| stats avg(responseTime) as avgResponseTime
```

### CloudWatch Metrics

#### ECS Service Metrics
- **CPUUtilization**: CPU usage percentage
- **MemoryUtilization**: Memory usage percentage
- **DesiredTaskCount**: Number of desired tasks
- **RunningTaskCount**: Number of running tasks
- **PendingTaskCount**: Number of pending tasks

#### View Metrics
```bash
# Get CPU utilization
aws cloudwatch get-metric-statistics \
  --namespace AWS/ECS \
  --metric-name CPUUtilization \
  --dimensions Name=ServiceName,Value=bankmanagementsystem-service \
               Name=ClusterName,Value=production-cluster \
  --start-time 2024-01-01T00:00:00Z \
  --end-time 2024-01-01T23:59:59Z \
  --period 3600 \
  --statistics Average \
  --region us-east-1
```

### CloudWatch Alarms

#### Create CPU Alarm
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name bankmanagementsystem-high-cpu \
  --alarm-description "Alert when CPU exceeds 80%" \
  --metric-name CPUUtilization \
  --namespace AWS/ECS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --dimensions Name=ServiceName,Value=bankmanagementsystem-service \
               Name=ClusterName,Value=production-cluster \
  --region us-east-1
```

#### Create Memory Alarm
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name bankmanagementsystem-high-memory \
  --alarm-description "Alert when memory exceeds 80%" \
  --metric-name MemoryUtilization \
  --namespace AWS/ECS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --dimensions Name=ServiceName,Value=bankmanagementsystem-service \
               Name=ClusterName,Value=production-cluster \
  --region us-east-1
```

### Application Performance Monitoring

For advanced monitoring, consider integrating:
- **AWS X-Ray**: Distributed tracing
- **Application Insights**: .NET application monitoring
- **Datadog/New Relic**: Third-party APM solutions

---

## Troubleshooting

### Common Issues and Solutions

#### 1. Task Fails to Start

**Symptoms**:
- Tasks transition from PENDING to STOPPED
- Service never reaches desired count

**Possible Causes and Solutions**:

**a) Image Pull Errors**
```bash
# Check task stopped reason
aws ecs describe-tasks \
  --cluster production-cluster \
  --tasks <task-id> \
  --query 'tasks[0].stoppedReason'

# Solution: Verify ECR permissions
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
```

**b) Invalid CPU/Memory Combination**
```
Error: Invalid CPU or memory value specified
```
Solution: Use valid Fargate combinations (see ECS Task Definition section)

**c) Insufficient ENI Capacity**
```
Error: No available network interfaces
```
Solution: Increase subnet size or use additional subnets

#### 2. Health Check Failures

**Symptoms**:
- Tasks start but are marked unhealthy
- Load balancer shows targets as unhealthy

**Solutions**:

**a) Verify Health Endpoint**
```bash
# Get task public IP
TASK_IP=$(aws ecs describe-tasks \
  --cluster production-cluster \
  --tasks <task-id> \
  --query 'tasks[0].attachments[0].details[?name==`privateIPv4Address`].value' \
  --output text)

# Test health endpoint
curl http://$TASK_IP:8080/health/
```

**b) Check Security Group Rules**
```bash
# Verify inbound rules allow health check port
aws ec2 describe-security-groups \
  --group-ids <security-group-id> \
  --query 'SecurityGroups[0].IpPermissions'
```

**c) Increase Health Check Grace Period**
Edit `ecs/service-definition.json`:
```json
{
  "healthCheckGracePeriodSeconds": 600
}
```

#### 3. Service Update Failures

**Symptoms**:
- Service update stuck in progress
- Old tasks not being replaced

**Solutions**:

**a) Force New Deployment**
```bash
aws ecs update-service \
  --cluster production-cluster \
  --service bankmanagementsystem-service \
  --force-new-deployment \
  --region us-east-1
```

**b) Check Deployment Circuit Breaker**
```bash
aws ecs describe-services \
  --cluster production-cluster \
  --services bankmanagementsystem-service \
  --query 'services[0].deployments'
```

#### 4. High Memory Usage

**Symptoms**:
- Tasks being killed due to OOM
- MemoryUtilization approaching 100%

**Solutions**:

**a) Increase Task Memory**
Edit `ecs/task-definition.json`:
```json
{
  "cpu": "1024",
  "memory": "2048"
}
```

**b) Optimize .NET Memory Settings**
Add environment variables:
```json
{
  "environment": [
    {
      "name": "DOTNET_GCHeapHardLimit",
      "value": "800000000"
    }
  ]
}
```

#### 5. Network Connectivity Issues

**Symptoms**:
- Tasks cannot reach external services
- Database connection failures

**Solutions**:

**a) Verify NAT Gateway (for private subnets)**
```bash
# Check route table
aws ec2 describe-route-tables \
  --filters "Name=association.subnet-id,Values=<subnet-id>"
```

**b) Check Security Group Outbound Rules**
```bash
aws ec2 describe-security-groups \
  --group-ids <security-group-id> \
  --query 'SecurityGroups[0].IpPermissionsEgress'
```

**c) Test Connectivity from Task**
```bash
# Execute command in running task
aws ecs execute-command \
  --cluster production-cluster \
  --task <task-id> \
  --container bankmanagementsystem \
  --interactive \
  --command "/bin/bash"

# Inside container
curl -v http://external-service.com
```

### Debugging Commands

#### View Task Logs
```bash
# Get task ARN
TASK_ARN=$(aws ecs list-tasks \
  --cluster production-cluster \
  --service-name bankmanagementsystem-service \
  --query 'taskArns[0]' --output text)

# View logs
aws logs tail /ecs/bankmanagementsystem \
  --follow \
  --filter-pattern "task/${TASK_ARN##*/}"
```

#### Describe Task Details
```bash
aws ecs describe-tasks \
  --cluster production-cluster \
  --tasks <task-id> \
  --query 'tasks[0]' \
  --output json
```

#### Check Service Events
```bash
aws ecs describe-services \
  --cluster production-cluster \
  --services bankmanagementsystem-service \
  --query 'services[0].events[0:10]' \
  --output table
```

---

## Security Considerations

### 1. Container Security

#### Non-Root User
The Dockerfile creates and uses a non-root user:
```dockerfile
RUN groupadd -r bankapp && useradd -r -g bankapp bankapp
USER bankapp
```

#### Image Scanning
Enable ECR image scanning:
```bash
aws ecr put-image-scanning-configuration \
  --repository-name bankmanagementsystem \
  --image-scanning-configuration scanOnPush=true
```

View scan results:
```bash
aws ecr describe-image-scan-findings \
  --repository-name bankmanagementsystem \
  --image-id imageTag=latest
```

### 2. Network Security

#### Security Group Best Practices
- **Principle of Least Privilege**: Only allow necessary ports
- **Source Restrictions**: Limit inbound traffic to known sources
- **Regular Audits**: Review and update rules periodically

Example restrictive security group:
```bash
# Allow HTTP only from ALB security group
aws ec2 authorize-security-group-ingress \
  --group-id <task-sg-id> \
  --protocol tcp \
  --port 8080 \
  --source-group <alb-sg-id>

# Allow HTTPS outbound for external APIs
aws ec2 authorize-security-group-egress \
  --group-id <task-sg-id> \
  --protocol tcp \
  --port 443 \
  --cidr 0.0.0.0/0
```

#### VPC Endpoints
Use VPC endpoints to avoid internet traffic for AWS services:
```bash
# Create ECR endpoint
aws ec2 create-vpc-endpoint \
  --vpc-id <vpc-id> \
  --service-name com.amazonaws.us-east-1.ecr.dkr \
  --route-table-ids <route-table-id>

# Create S3 endpoint (for ECR layers)
aws ec2 create-vpc-endpoint \
  --vpc-id <vpc-id> \
  --service-name com.amazonaws.us-east-1.s3 \
  --route-table-ids <route-table-id>
```

### 3. IAM Security

#### Task Execution Role Permissions
Limit permissions to minimum required:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "arn:aws:logs:*:*:log-group:/ecs/bankmanagementsystem:*"
    }
  ]
}
```

#### Task Role Permissions
Grant only necessary AWS service access:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject"
      ],
      "Resource": "arn:aws:s3:::my-app-bucket/*"
    }
  ]
}
```

### 4. Secrets Management

#### Use AWS Secrets Manager
```bash
# Store database password
aws secretsmanager create-secret \
  --name bankmanagementsystem/db-credentials \
  --secret-string '{"username":"dbuser","password":"SecurePass123"}'

# Reference in task definition
{
  "secrets": [
    {
      "name": "DB_USERNAME",
      "valueFrom": "arn:aws:secretsmanager:region:account:secret:bankmanagementsystem/db-credentials:username::"
    },
    {
      "name": "DB_PASSWORD",
      "valueFrom": "arn:aws:secretsmanager:region:account:secret:bankmanagementsystem/db-credentials:password::"
    }
  ]
}
```

### 5. Compliance and Auditing

#### Enable CloudTrail
```bash
aws cloudtrail create-trail \
  --name bankmanagementsystem-trail \
  --s3-bucket-name my-cloudtrail-bucket

aws cloudtrail start-logging --name bankmanagementsystem-trail
```

#### Enable VPC Flow Logs
```bash
aws ec2 create-flow-logs \
  --resource-type VPC \
  --resource-ids <vpc-id> \
  --traffic-type ALL \
  --log-destination-type cloud-watch-logs \
  --log-group-name /aws/vpc/flowlogs
```

---

## Scaling and Performance

### Auto Scaling Configuration

#### Service Auto Scaling
Enable auto scaling based on CPU or memory:

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/production-cluster/bankmanagementsystem-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10

# Create scaling policy (CPU-based)
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/production-cluster/bankmanagementsystem-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 70.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
    },
    "ScaleInCooldown": 300,
    "ScaleOutCooldown": 60
  }'

# Create scaling policy (Memory-based)
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/production-cluster/bankmanagementsystem-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name memory-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 80.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageMemoryUtilization"
    },
    "ScaleInCooldown": 300,
    "ScaleOutCooldown": 60
  }'
```

### Performance Optimization

#### 1. .NET Runtime Optimization

Add environment variables for performance:
```json
{
  "environment": [
    {
      "name": "DOTNET_TieredCompilation",
      "value": "1"
    },
    {
      "name": "DOTNET_ReadyToRun",
      "value": "1"
    },
    {
      "name": "DOTNET_TC_QuickJitForLoops",
      "value": "1"
    }
  ]
}
```

#### 2. Container Resource Limits

Optimize CPU and memory allocation:
- Start with baseline: 512 CPU, 1024 MB memory
- Monitor actual usage with CloudWatch
- Adjust based on 80% utilization target
- Consider burst capacity needs

#### 3. Connection Pooling

Configure database connection pooling:
```csharp
// In connection string
"Max Pool Size=100;Min Pool Size=10;Connection Lifetime=300"
```

#### 4. Caching Strategy

Implement caching for frequently accessed data:
- Use in-memory caching for static data
- Consider Redis/ElastiCache for distributed caching
- Set appropriate TTL values

### Load Testing

#### Using Apache Bench
```bash
# Install Apache Bench
sudo apt-get install apache2-utils

# Run load test
ab -n 10000 -c 100 http://<alb-dns>/health/
```

#### Using Artillery
```bash
# Install Artillery
npm install -g artillery

# Create test scenario (load-test.yml)
config:
  target: 'http://<alb-dns>'
  phases:
    - duration: 60
      arrivalRate: 10
scenarios:
  - flow:
      - get:
          url: '/health/'

# Run test
artillery run load-test.yml
```

### Blue/Green Deployment

Enable blue/green deployments for zero-downtime updates:

```bash
# Update service with deployment controller
aws ecs create-service \
  --cluster production-cluster \
  --service-name bankmanagementsystem-service \
  --task-definition bankmanagementsystem-task \
  --desired-count 2 \
  --launch-type FARGATE \
  --deployment-controller type=CODE_DEPLOY \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxx],securityGroups=[sg-xxx],assignPublicIp=ENABLED}"
```

Configure CodeDeploy for blue/green:
1. Create CodeDeploy application
2. Create deployment group
3. Configure traffic shifting (linear, canary, or all-at-once)
4. Set up rollback triggers

---

## Additional Resources

### AWS Documentation
- [ECS Fargate Documentation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)
- [ECS Task Definitions](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_definitions.html)
- [ECS Service Auto Scaling](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/service-auto-scaling.html)
- [CloudWatch Logs](https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/WhatIsCloudWatchLogs.html)

### .NET Resources
- [.NET 6.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6)
- [Containerizing .NET Applications](https://docs.microsoft.com/en-us/dotnet/core/docker/introduction)
- [.NET Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/core/performance/)

### Docker Resources
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Multi-Stage Builds](https://docs.docker.com/develop/develop-images/multistage-build/)
- [Docker Security](https://docs.docker.com/engine/security/)

---

## Support and Maintenance

### Regular Maintenance Tasks

#### Weekly
- Review CloudWatch logs for errors
- Check service health and task count
- Monitor resource utilization

#### Monthly
- Update Docker base images
- Review and update security groups
- Analyze cost optimization opportunities
- Update dependencies and packages

#### Quarterly
- Conduct security audit
- Review and update IAM policies
- Performance testing and optimization
- Disaster recovery testing

### Getting Help

For issues or questions:
1. Check CloudWatch Logs for error messages
2. Review this deployment guide
3. Consult AWS documentation
4. Contact AWS Support (if applicable)
5. Review application logs and metrics

---

## Conclusion

This deployment guide provides comprehensive instructions for containerizing and deploying the BankManagementSystem application on AWS ECS Fargate. By following these guidelines, you can achieve:

- **Scalability**: Auto-scaling based on demand
- **High Availability**: Multi-AZ deployment with load balancing
- **Security**: IAM roles, security groups, and secrets management
- **Observability**: CloudWatch logs and metrics
- **Cost Optimization**: Pay only for resources used with Fargate

For production deployments, ensure you:
- Implement proper monitoring and alerting
- Configure auto-scaling policies
- Set up backup and disaster recovery
- Follow security best practices
- Regularly update and patch dependencies

**Happy Deploying! 🚀**
