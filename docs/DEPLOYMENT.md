# BankManagementSystem - AWS ECS Fargate Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Architecture](#architecture)
4. [Local Development](#local-development)
5. [Building and Pushing Docker Images](#building-and-pushing-docker-images)
6. [AWS ECS Fargate Deployment](#aws-ecs-fargate-deployment)
7. [Configuration Management](#configuration-management)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Troubleshooting](#troubleshooting)
10. [Security Considerations](#security-considerations)

---

## Overview

This guide provides comprehensive instructions for containerizing and deploying the BankManagementSystem .NET 6.0 Windows Forms application to AWS ECS Fargate using Windows containers.

### Application Details
- **Technology**: .NET 6.0 Windows Forms Application
- **Container Type**: Windows Server Core LTSC 2022
- **Health Check Port**: 8080
- **Health Endpoint**: `/health`
- **Target Platform**: AWS ECS Fargate

### Key Features
- Multi-stage Docker build for optimized image size
- Health check endpoint for container orchestration
- CloudWatch logging integration
- Auto-scaling support
- Load balancer integration

---

## Prerequisites

### Required Software
1. **Docker Desktop for Windows**
   - Version: 4.x or later
   - Windows containers enabled
   - Download: https://www.docker.com/products/docker-desktop

2. **AWS CLI**
   - Version: 2.x or later
   - Configured with appropriate credentials
   - Install: `winget install Amazon.AWSCLI`

3. **.NET 6.0 SDK**
   - Required for local development
   - Download: https://dotnet.microsoft.com/download/dotnet/6.0

4. **Git**
   - For version control
   - Download: https://git-scm.com/downloads

### AWS Requirements

#### IAM Roles
You need two IAM roles for ECS Fargate:

**1. ECS Task Execution Role** (`ecsTaskExecutionRole`)
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
        "ecr:BatchGetImage",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "*"
    }
  ]
}
```

**2. ECS Task Role** (`ecsTaskRole`)
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "secretsmanager:GetSecretValue",
        "kms:Decrypt"
      ],
      "Resource": "*"
    }
  ]
}
```

#### Network Requirements
- **VPC**: A VPC with at least 2 subnets in different availability zones
- **Security Group**: Allow inbound traffic on port 8080 (health check)
- **Internet Gateway**: For public IP assignment (if using ENABLED for assignPublicIp)

#### AWS Services
- **Amazon ECR**: Container registry for Docker images
- **Amazon ECS**: Container orchestration service
- **CloudWatch Logs**: For application logging
- **Application Load Balancer** (optional): For distributing traffic

---

## Architecture

### Container Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Load Balancer                │
│                         (Optional)                           │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                      ECS Service                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              ECS Task (Fargate)                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │   BankManagementSystem Container               │  │  │
│  │  │   - .NET 6.0 Runtime                           │  │  │
│  │  │   - Windows Server Core LTSC 2022              │  │  │
│  │  │   - Health Check Endpoint (:8080/health)       │  │  │
│  │  │   - Application Logs → CloudWatch              │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   CloudWatch Logs                            │
│              /ecs/bankmanagement-system                      │
└─────────────────────────────────────────────────────────────┘
```

### Multi-Stage Docker Build

```dockerfile
Stage 1: Builder (SDK Image)
- Base: mcr.microsoft.com/dotnet/sdk:6.0-windowsservercore-ltsc2022
- Purpose: Compile and publish the application
- Output: Published binaries in /app/publish

Stage 2: Runtime (Runtime Image)
- Base: mcr.microsoft.com/dotnet/runtime:6.0-windowsservercore-ltsc2022
- Purpose: Run the application
- Size: Optimized (no SDK overhead)
```

---

## Local Development

### Building Locally

1. **Clone the repository**
```bash
git clone <repository-url>
cd BankManagementSystem
```

2. **Restore dependencies**
```bash
dotnet restore BankManagementSystem.sln
```

3. **Build the solution**
```bash
dotnet build BankManagementSystem.sln -c Release
```

4. **Run the application**
```bash
cd BankManagementSystem
dotnet run
```

### Testing with Docker Compose

1. **Build and run with Docker Compose**
```bash
docker-compose up --build
```

2. **Access the health endpoint**
```bash
curl http://localhost:8080/health
```

Expected response:
```json
{
  "status": "healthy",
  "service": "BankManagementSystem"
}
```

3. **View logs**
```bash
docker-compose logs -f bankmanagement-app
```

4. **Stop the application**
```bash
docker-compose down
```

---

## Building and Pushing Docker Images

### Option 1: Using Build Script (Recommended)

#### Linux/macOS
```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Windows
```cmd
scripts\build-push.bat
```

The script will:
1. Prompt for registry selection (AWS ECR or Docker Hub)
2. Authenticate with the selected registry
3. Build the Docker image
4. Tag the image appropriately
5. Push to the registry

### Option 2: Manual Build and Push

#### AWS ECR

1. **Authenticate with ECR**
```bash
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
```

2. **Create ECR repository** (if not exists)
```bash
aws ecr create-repository --repository-name bankmanagement-system --region us-east-1
```

3. **Build the image**
```bash
docker build -t bankmanagement-system:latest .
```

4. **Tag the image**
```bash
docker tag bankmanagement-system:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/bankmanagement-system:latest
```

5. **Push the image**
```bash
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/bankmanagement-system:latest
```

#### Docker Hub

1. **Login to Docker Hub**
```bash
docker login
```

2. **Build the image**
```bash
docker build -t <username>/bankmanagement-system:latest .
```

3. **Push the image**
```bash
docker push <username>/bankmanagement-system:latest
```

---

## AWS ECS Fargate Deployment

### Prerequisites Setup

#### 1. Create VPC and Networking (if not exists)

```bash
# Create VPC
aws ec2 create-vpc --cidr-block 10.0.0.0/16 --region us-east-1

# Create subnets in different AZs
aws ec2 create-subnet --vpc-id <vpc-id> --cidr-block 10.0.1.0/24 --availability-zone us-east-1a
aws ec2 create-subnet --vpc-id <vpc-id> --cidr-block 10.0.2.0/24 --availability-zone us-east-1b

# Create Internet Gateway
aws ec2 create-internet-gateway
aws ec2 attach-internet-gateway --vpc-id <vpc-id> --internet-gateway-id <igw-id>

# Create route table and associate with subnets
aws ec2 create-route-table --vpc-id <vpc-id>
aws ec2 create-route --route-table-id <rtb-id> --destination-cidr-block 0.0.0.0/0 --gateway-id <igw-id>
```

#### 2. Create Security Group

```bash
# Create security group
aws ec2 create-security-group \
  --group-name bankmanagement-sg \
  --description "Security group for BankManagementSystem" \
  --vpc-id <vpc-id>

# Allow inbound traffic on port 8080
aws ec2 authorize-security-group-ingress \
  --group-id <sg-id> \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0

# Allow inbound traffic on port 80 (if using ALB)
aws ec2 authorize-security-group-ingress \
  --group-id <sg-id> \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0
```

#### 3. Create IAM Roles

**Create Task Execution Role:**
```bash
aws iam create-role \
  --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://trust-policy.json

aws iam attach-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

**Create Task Role:**
```bash
aws iam create-role \
  --role-name ecsTaskRole \
  --assume-role-policy-document file://trust-policy.json

aws iam attach-role-policy \
  --role-name ecsTaskRole \
  --policy-arn <your-custom-policy-arn>
```

### Deployment Using Script (Recommended)

#### Linux/macOS
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows
```cmd
scripts\deploy-image.bat
```

The script will:
1. Prompt for AWS region and cluster name
2. Create ECS cluster (if not exists)
3. Prompt for network configuration (VPC, subnets, security group)
4. Prompt for Docker image URI
5. Optionally create Application Load Balancer
6. Create CloudWatch log group
7. Register task definition
8. Create or update ECS service
9. Wait for service to stabilize
10. Display deployment information

### Manual Deployment

#### 1. Create CloudWatch Log Group
```bash
aws logs create-log-group --log-group-name /ecs/bankmanagement-system --region us-east-1
```

#### 2. Register Task Definition
```bash
aws ecs register-task-definition --cli-input-json file://ecs/task-definition.json --region us-east-1
```

#### 3. Create ECS Cluster
```bash
aws ecs create-cluster --cluster-name bankmanagement-cluster --region us-east-1
```

#### 4. Create ECS Service
```bash
aws ecs create-service --cli-input-json file://ecs/service-definition.json --region us-east-1
```

#### 5. Verify Deployment
```bash
aws ecs describe-services \
  --cluster bankmanagement-cluster \
  --services bankmanagement-system-service \
  --region us-east-1
```

---

## Configuration Management

### Environment Variables

The application supports the following environment variables:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `HEALTH_CHECK_PORT` | Port for health check endpoint | 8080 | No |
| `STARTUP_USER` | User context for application | containeruser | No |
| `DOTNET_RUNNING_IN_CONTAINER` | Indicates container environment | true | No |
| `LOG_LEVEL` | Logging level | Information | No |
| `DB_CONNECTION_STRING` | Database connection string | - | Yes |

### Updating Configuration

#### Update Task Definition
1. Modify `ecs/task-definition.json`
2. Register new task definition revision
```bash
aws ecs register-task-definition --cli-input-json file://ecs/task-definition.json
```

#### Update Service
```bash
aws ecs update-service \
  --cluster bankmanagement-cluster \
  --service bankmanagement-system-service \
  --task-definition bankmanagement-system-task:2 \
  --force-new-deployment
```

### Secrets Management

For sensitive data, use AWS Secrets Manager:

1. **Create secret**
```bash
aws secretsmanager create-secret \
  --name bankmanagement/db-password \
  --secret-string "your-password"
```

2. **Reference in task definition**
```json
{
  "secrets": [
    {
      "name": "DB_PASSWORD",
      "valueFrom": "arn:aws:secretsmanager:region:account-id:secret:bankmanagement/db-password"
    }
  ]
}
```

---

## Monitoring and Logging

### CloudWatch Logs

#### View Logs
```bash
# Tail logs in real-time
aws logs tail /ecs/bankmanagement-system --follow --region us-east-1

# View logs for specific time range
aws logs filter-log-events \
  --log-group-name /ecs/bankmanagement-system \
  --start-time 1609459200000 \
  --end-time 1609545600000
```

#### Log Insights Queries

**Error Analysis:**
```sql
fields @timestamp, @message
| filter @message like /ERROR/
| sort @timestamp desc
| limit 100
```

**Performance Monitoring:**
```sql
fields @timestamp, @message
| filter @message like /health/
| stats count() by bin(5m)
```

### CloudWatch Metrics

Key metrics to monitor:
- **CPUUtilization**: Task CPU usage
- **MemoryUtilization**: Task memory usage
- **TargetResponseTime**: ALB response time
- **HealthyHostCount**: Number of healthy targets

#### Create CloudWatch Dashboard
```bash
aws cloudwatch put-dashboard \
  --dashboard-name BankManagementSystem \
  --dashboard-body file://dashboard.json
```

### Application Insights (Optional)

For advanced monitoring, integrate Application Insights:

1. Add NuGet package:
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

2. Configure in application
3. Update environment variables with instrumentation key

---

## Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptoms:**
- Tasks transition to STOPPED state immediately
- Error: "CannotPullContainerError"

**Solutions:**
- Verify ECR authentication
- Check task execution role permissions
- Ensure image URI is correct
- Verify network connectivity to ECR

```bash
# Check task stopped reason
aws ecs describe-tasks \
  --cluster bankmanagement-cluster \
  --tasks <task-id> \
  --query 'tasks[0].stoppedReason'
```

#### 2. Health Check Failures

**Symptoms:**
- Tasks fail health checks
- Service unable to reach steady state

**Solutions:**
- Verify health endpoint is accessible: `curl http://task-ip:8080/health`
- Check security group allows traffic on port 8080
- Increase health check grace period
- Review application logs for startup errors

```bash
# Get task IP
aws ecs describe-tasks \
  --cluster bankmanagement-cluster \
  --tasks <task-id> \
  --query 'tasks[0].attachments[0].details[?name==`privateIPv4Address`].value'
```

#### 3. Out of Memory Errors

**Symptoms:**
- Tasks stop with exit code 137
- Error: "OutOfMemoryError"

**Solutions:**
- Increase memory allocation in task definition
- Optimize application memory usage
- Review memory leaks

Valid Fargate CPU/Memory combinations:
- CPU: 512 → Memory: 1024, 2048, 3072, 4096
- CPU: 1024 → Memory: 2048-8192 (increments of 1024)
- CPU: 2048 → Memory: 4096-16384 (increments of 1024)

#### 4. Service Not Stabilizing

**Symptoms:**
- Service stuck in deployment
- Tasks continuously starting and stopping

**Solutions:**
- Check CloudWatch logs for application errors
- Verify environment variables are correct
- Ensure database connectivity
- Review task definition configuration

```bash
# Force new deployment
aws ecs update-service \
  --cluster bankmanagement-cluster \
  --service bankmanagement-system-service \
  --force-new-deployment
```

#### 5. Load Balancer Issues

**Symptoms:**
- 502/503 errors from ALB
- Targets marked unhealthy

**Solutions:**
- Verify target group health check configuration
- Ensure security group allows ALB → Task traffic
- Check health endpoint returns 200 status
- Increase deregistration delay

```bash
# Check target health
aws elbv2 describe-target-health \
  --target-group-arn <target-group-arn>
```

### Debugging Commands

```bash
# List running tasks
aws ecs list-tasks --cluster bankmanagement-cluster --service-name bankmanagement-system-service

# Describe task details
aws ecs describe-tasks --cluster bankmanagement-cluster --tasks <task-id>

# View service events
aws ecs describe-services \
  --cluster bankmanagement-cluster \
  --services bankmanagement-system-service \
  --query 'services[0].events[0:10]'

# Execute command in running container (ECS Exec)
aws ecs execute-command \
  --cluster bankmanagement-cluster \
  --task <task-id> \
  --container bankmanagement-system \
  --interactive \
  --command "powershell"
```

---

## Security Considerations

### Container Security

1. **Use Non-Root User**
   - The Dockerfile creates and uses a non-admin user (`appuser`)
   - Limits potential damage from container compromise

2. **Minimal Base Image**
   - Uses Windows Server Core (not full Windows Server)
   - Reduces attack surface

3. **Read-Only Root Filesystem** (where possible)
   - Prevents unauthorized file modifications

### Network Security

1. **Security Groups**
   - Restrict inbound traffic to necessary ports only
   - Use least privilege principle

2. **Private Subnets** (recommended for production)
   - Place tasks in private subnets
   - Use NAT Gateway for outbound connectivity

3. **VPC Endpoints**
   - Use VPC endpoints for AWS services (ECR, CloudWatch, Secrets Manager)
   - Reduces data transfer costs and improves security

### Secrets Management

1. **Never Hardcode Secrets**
   - Use AWS Secrets Manager or Parameter Store
   - Reference secrets in task definition

2. **Rotate Credentials Regularly**
   - Enable automatic rotation for database passwords
   - Update task definition with new secret ARNs

3. **Encrypt at Rest**
   - Enable encryption for CloudWatch Logs
   - Use KMS for secret encryption

### IAM Best Practices

1. **Least Privilege**
   - Grant only necessary permissions to task roles
   - Use separate roles for different services

2. **Task Execution Role**
   - Only for pulling images and writing logs
   - Do not grant application permissions

3. **Task Role**
   - For application-level AWS API calls
   - Scope permissions to specific resources

### Compliance

1. **Logging and Auditing**
   - Enable CloudTrail for API call logging
   - Retain logs for compliance requirements

2. **Vulnerability Scanning**
   - Use ECR image scanning
   - Regularly update base images

3. **Access Control**
   - Use IAM policies to control ECS access
   - Enable MFA for sensitive operations

---

## Scaling and Performance

### Auto Scaling

#### Target Tracking Scaling
```bash
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/bankmanagement-cluster/bankmanagement-system-service \
  --min-capacity 2 \
  --max-capacity 10

aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/bankmanagement-cluster/bankmanagement-system-service \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json
```

**scaling-policy.json:**
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

### Performance Optimization

1. **Resource Allocation**
   - Start with CPU: 512, Memory: 1024
   - Monitor and adjust based on actual usage

2. **Connection Pooling**
   - Configure database connection pooling
   - Reuse HTTP connections

3. **Caching**
   - Implement application-level caching
   - Use Redis/ElastiCache for distributed caching

4. **Health Check Tuning**
   - Adjust interval and timeout based on application startup time
   - Use appropriate grace period

---

## Cost Optimization

### Fargate Pricing

Fargate pricing is based on:
- vCPU hours
- Memory (GB) hours

**Example Calculation:**
- CPU: 0.5 vCPU = $0.04048 per hour
- Memory: 1 GB = $0.004445 per hour
- Total: ~$0.045 per hour per task

### Cost Reduction Strategies

1. **Right-Size Resources**
   - Monitor actual CPU/memory usage
   - Reduce allocation if over-provisioned

2. **Use Fargate Spot** (for non-critical workloads)
   - Up to 70% cost savings
   - Tasks may be interrupted

3. **Optimize Image Size**
   - Use multi-stage builds
   - Remove unnecessary dependencies
   - Reduces pull time and storage costs

4. **Schedule Scaling**
   - Scale down during off-peak hours
   - Use scheduled scaling policies

5. **Use Savings Plans**
   - Commit to consistent usage
   - Save up to 50% on compute costs

---

## Disaster Recovery

### Backup Strategy

1. **Container Images**
   - Maintain multiple image versions in ECR
   - Enable ECR lifecycle policies

2. **Configuration**
   - Version control all IaC files
   - Store task definitions in S3

3. **Data**
   - Regular database backups
   - Use RDS automated backups

### Recovery Procedures

#### Rollback Deployment
```bash
# List task definition revisions
aws ecs list-task-definitions --family-prefix bankmanagement-system-task

# Update service to previous revision
aws ecs update-service \
  --cluster bankmanagement-cluster \
  --service bankmanagement-system-service \
  --task-definition bankmanagement-system-task:1
```

#### Multi-Region Deployment

For high availability:
1. Deploy to multiple AWS regions
2. Use Route 53 for DNS failover
3. Replicate ECR images across regions
4. Configure cross-region database replication

---

## Additional Resources

### Documentation
- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [AWS Fargate Documentation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)
- [.NET on AWS](https://aws.amazon.com/developer/language/net/)

### Tools
- [AWS Copilot CLI](https://aws.github.io/copilot-cli/) - Simplified ECS deployment
- [ECS CLI](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/ECS_CLI.html) - Command-line interface for ECS

### Support
- AWS Support: https://console.aws.amazon.com/support/
- AWS Forums: https://forums.aws.amazon.com/
- Stack Overflow: Tag `amazon-ecs`

---

## Conclusion

This deployment guide provides a comprehensive approach to containerizing and deploying the BankManagementSystem application to AWS ECS Fargate. Follow the best practices outlined in this document to ensure a secure, scalable, and cost-effective deployment.

For questions or issues, please refer to the troubleshooting section or contact your DevOps team.

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Maintained By**: DevOps Team
