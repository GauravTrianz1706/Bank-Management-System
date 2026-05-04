# BankManagementSystem - AWS ECS Fargate Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Architecture](#architecture)
4. [Local Development](#local-development)
5. [AWS ECS Fargate Setup](#aws-ecs-fargate-setup)
6. [Building and Pushing Docker Images](#building-and-pushing-docker-images)
7. [Deploying to AWS ECS](#deploying-to-aws-ecs)
8. [Configuration Management](#configuration-management)
9. [Monitoring and Logging](#monitoring-and-logging)
10. [Troubleshooting](#troubleshooting)
11. [Security Considerations](#security-considerations)
12. [Scaling and Performance](#scaling-and-performance)

---

## Overview

BankManagementSystem is a .NET 8.0 Windows Forms application that has been containerized for deployment on AWS ECS Fargate. The application includes an embedded HTTP health check endpoint for container orchestration support.

### Technology Stack
- **.NET Version**: 8.0
- **Application Type**: Windows Forms with embedded HTTP server
- **Health Check Port**: 8080
- **Health Endpoint**: `/health`
- **Database**: SQL Server (external)
- **Container Runtime**: Docker
- **Deployment Platform**: AWS ECS Fargate

---

## Prerequisites

### Required Tools
1. **Docker Desktop** (version 20.10 or later)
   - Download: https://www.docker.com/products/docker-desktop
   - Ensure Docker is running before building images

2. **AWS CLI** (version 2.x)
   - Installation: https://aws.amazon.com/cli/
   - Configure with credentials: `aws configure`

3. **.NET 8.0 SDK** (for local development)
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0

4. **Git** (for version control)
   - Download: https://git-scm.com/downloads

### AWS Account Requirements
- Active AWS account with appropriate permissions
- IAM user with permissions for:
  - ECS (create/update clusters, services, task definitions)
  - ECR (create repositories, push images)
  - EC2 (VPC, subnets, security groups)
  - CloudWatch Logs (create log groups)
  - IAM (create/manage roles)
  - Elastic Load Balancing (create ALB, target groups)

### Network Requirements
- VPC with at least 2 subnets in different availability zones
- Security group allowing:
  - Inbound: Port 8080 (application), Port 80 (ALB if used)
  - Outbound: All traffic (for database and external services)

---

## Architecture

### Application Architecture
```
┌─────────────────────────────────────────────────────────────┐
│                     AWS ECS Fargate                         │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              Application Load Balancer                │  │
│  │                  (Port 80 HTTP)                       │  │
│  └────────────────────┬──────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────┴──────────────────────────────────┐  │
│  │              Target Group (IP mode)                   │  │
│  │           Health Check: /health (Port 8080)           │  │
│  └────────────────────┬──────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────┴──────────────────────────────────┐  │
│  │         ECS Service (Fargate Launch Type)             │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │  Task 1: BankManagementSystem Container         │  │  │
│  │  │  - Port 8080 (Health Check)                     │  │  │
│  │  │  - .NET 8.0 Runtime                             │  │  │
│  │  │  - CPU: 512, Memory: 1024 MB                    │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │  Task 2: BankManagementSystem Container         │  │  │
│  │  │  - Port 8080 (Health Check)                     │  │  │
│  │  │  - .NET 8.0 Runtime                             │  │  │
│  │  │  - CPU: 512, Memory: 1024 MB                    │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌──────────────────┐
                    │  SQL Server DB   │
                    │   (External)     │
                    └──────────────────┘
```

### Container Architecture
- **Multi-stage Dockerfile**: Optimized build with SDK and runtime stages
- **Base Images**: 
  - Build: `mcr.microsoft.com/dotnet/sdk:8.0`
  - Runtime: `mcr.microsoft.com/dotnet/runtime:8.0`
- **Security**: Non-root user execution
- **Health Check**: Embedded HTTP endpoint at `/health`

---

## Local Development

### Building Locally
```bash
# Clone the repository
git clone <repository-url>
cd abcc

# Build the Docker image
docker build -t bankmanagement-system:local .

# Run locally
docker run -d \
  -p 8080:8080 \
  -e HEALTH_CHECK_PORT=8080 \
  -e CURRENT_USER=localuser \
  -e DB_HOST=your-db-host \
  -e DB_NAME=OpenBankLocal \
  -e DB_USER=your-db-user \
  -e DB_PASSWORD=your-db-password \
  --name bankmanagement-local \
  bankmanagement-system:local

# Check health
curl http://localhost:8080/health

# View logs
docker logs -f bankmanagement-local

# Stop container
docker stop bankmanagement-local
docker rm bankmanagement-local
```

### Using Docker Compose
```bash
# Start the application
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the application
docker-compose down
```

---

## AWS ECS Fargate Setup

### Step 1: Create IAM Roles

#### ECS Task Execution Role
This role allows ECS to pull images from ECR and write logs to CloudWatch.

```bash
# Create trust policy file
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

# Create the role
aws iam create-role \
  --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach AWS managed policy
aws iam attach-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

#### ECS Task Role (Optional)
This role grants permissions to the application running in the container.

```bash
# Create the role
aws iam create-role \
  --role-name ecsTaskRole \
  --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach custom policies as needed (e.g., S3, DynamoDB access)
```

### Step 2: Create VPC and Network Resources

If you don't have an existing VPC:

```bash
# Create VPC
VPC_ID=$(aws ec2 create-vpc \
  --cidr-block 10.0.0.0/16 \
  --query 'Vpc.VpcId' \
  --output text)

# Create Internet Gateway
IGW_ID=$(aws ec2 create-internet-gateway \
  --query 'InternetGateway.InternetGatewayId' \
  --output text)

aws ec2 attach-internet-gateway \
  --vpc-id $VPC_ID \
  --internet-gateway-id $IGW_ID

# Create subnets in different AZs
SUBNET_1=$(aws ec2 create-subnet \
  --vpc-id $VPC_ID \
  --cidr-block 10.0.1.0/24 \
  --availability-zone us-east-1a \
  --query 'Subnet.SubnetId' \
  --output text)

SUBNET_2=$(aws ec2 create-subnet \
  --vpc-id $VPC_ID \
  --cidr-block 10.0.2.0/24 \
  --availability-zone us-east-1b \
  --query 'Subnet.SubnetId' \
  --output text)

# Create route table
ROUTE_TABLE=$(aws ec2 create-route-table \
  --vpc-id $VPC_ID \
  --query 'RouteTable.RouteTableId' \
  --output text)

aws ec2 create-route \
  --route-table-id $ROUTE_TABLE \
  --destination-cidr-block 0.0.0.0/0 \
  --gateway-id $IGW_ID

# Associate subnets with route table
aws ec2 associate-route-table \
  --subnet-id $SUBNET_1 \
  --route-table-id $ROUTE_TABLE

aws ec2 associate-route-table \
  --subnet-id $SUBNET_2 \
  --route-table-id $ROUTE_TABLE
```

### Step 3: Create Security Group

```bash
# Create security group
SG_ID=$(aws ec2 create-security-group \
  --group-name bankmanagement-sg \
  --description "Security group for BankManagementSystem" \
  --vpc-id $VPC_ID \
  --query 'GroupId' \
  --output text)

# Allow inbound traffic on port 8080 (application)
aws ec2 authorize-security-group-ingress \
  --group-id $SG_ID \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0

# Allow inbound traffic on port 80 (ALB)
aws ec2 authorize-security-group-ingress \
  --group-id $SG_ID \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0

# Allow all outbound traffic
aws ec2 authorize-security-group-egress \
  --group-id $SG_ID \
  --protocol -1 \
  --cidr 0.0.0.0/0
```

### Step 4: Create CloudWatch Log Group

```bash
aws logs create-log-group \
  --log-group-name /ecs/bankmanagement-system \
  --region us-east-1
```

### Step 5: Store Database Credentials in Secrets Manager

```bash
# Create secret for database password
aws secretsmanager create-secret \
  --name bankmanagement/db-password \
  --description "Database password for BankManagementSystem" \
  --secret-string "your-secure-password" \
  --region us-east-1
```

---

## Building and Pushing Docker Images

### Option 1: Using build-push.sh (Linux/macOS)

```bash
# Make script executable
chmod +x scripts/build-push.sh

# Run the script
./scripts/build-push.sh

# Follow the prompts:
# 1. Select registry type (1 for AWS ECR, 2 for Docker Hub)
# 2. Enter registry details
# 3. Enter image tag (default: latest)
```

### Option 2: Using build-push.bat (Windows)

```cmd
# Run the script
scripts\build-push.bat

# Follow the prompts
```

### Manual Build and Push (AWS ECR)

```bash
# Set variables
AWS_REGION=us-east-1
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
ECR_REPO=bankmanagement-system
IMAGE_TAG=latest

# Create ECR repository (if not exists)
aws ecr create-repository \
  --repository-name $ECR_REPO \
  --region $AWS_REGION

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | \
  docker login --username AWS --password-stdin \
  $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Build image
docker build -t $ECR_REPO:$IMAGE_TAG .

# Tag image
docker tag $ECR_REPO:$IMAGE_TAG \
  $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO:$IMAGE_TAG

# Push image
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO:$IMAGE_TAG
```

---

## Deploying to AWS ECS

### Option 1: Using deploy-image.sh (Linux/macOS)

```bash
# Make script executable
chmod +x scripts/deploy-image.sh

# Run the script
./scripts/deploy-image.sh

# Follow the prompts:
# 1. Enter AWS region
# 2. Enter ECS cluster name
# 3. Enter VPC and network details
# 4. Enter Docker image URI
# 5. Enter database configuration
# 6. Choose load balancer option
```

### Option 2: Using deploy-image.bat (Windows)

```cmd
# Run the script
scripts\deploy-image.bat

# Follow the prompts
```

### Manual Deployment

#### Register Task Definition

```bash
# Update placeholders in task definition
sed -i "s|{{IMAGE_URI}}|$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO:$IMAGE_TAG|g" \
  ecs/task-definition.json
sed -i "s|{{AWS_REGION}}|$AWS_REGION|g" ecs/task-definition.json
sed -i "s|{{ACCOUNT_ID}}|$AWS_ACCOUNT_ID|g" ecs/task-definition.json

# Register task definition
aws ecs register-task-definition \
  --cli-input-json file://ecs/task-definition.json \
  --region $AWS_REGION
```

#### Create ECS Cluster

```bash
aws ecs create-cluster \
  --cluster-name bankmanagement-cluster \
  --region $AWS_REGION
```

#### Create ECS Service

```bash
# Update service definition with your values
sed -i "s|{{CLUSTER_NAME}}|bankmanagement-cluster|g" ecs/service-definition.json
sed -i "s|{{SUBNET_1}}|$SUBNET_1|g" ecs/service-definition.json
sed -i "s|{{SUBNET_2}}|$SUBNET_2|g" ecs/service-definition.json
sed -i "s|{{SECURITY_GROUP}}|$SG_ID|g" ecs/service-definition.json

# Create service
aws ecs create-service \
  --cli-input-json file://ecs/service-definition.json \
  --region $AWS_REGION
```

---

## Configuration Management

### Environment Variables

The application uses the following environment variables:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `HEALTH_CHECK_PORT` | Port for health check endpoint | 8080 | Yes |
| `CURRENT_USER` | User context for application | containeruser | Yes |
| `DOTNET_RUNNING_IN_CONTAINER` | Indicates container environment | true | Yes |
| `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` | Globalization settings | false | Yes |
| `TZ` | Timezone | UTC | No |
| `LOG_LEVEL` | Logging level | Information | No |
| `DB_HOST` | Database server hostname | - | Yes |
| `DB_NAME` | Database name | OpenBankLocal | Yes |
| `DB_USER` | Database username | - | Yes |
| `DB_PASSWORD` | Database password (from Secrets Manager) | - | Yes |

### Updating Configuration

To update environment variables:

1. **Update Task Definition**: Modify `ecs/task-definition.json`
2. **Register New Revision**: Run `aws ecs register-task-definition`
3. **Update Service**: Run `aws ecs update-service` with new task definition

```bash
# Update service with new task definition
aws ecs update-service \
  --cluster bankmanagement-cluster \
  --service bankmanagement-system-service \
  --task-definition bankmanagement-system-task:2 \
  --force-new-deployment \
  --region $AWS_REGION
```

---

## Monitoring and Logging

### CloudWatch Logs

View application logs:

```bash
# Tail logs in real-time
aws logs tail /ecs/bankmanagement-system --follow --region $AWS_REGION

# View logs for specific time range
aws logs filter-log-events \
  --log-group-name /ecs/bankmanagement-system \
  --start-time $(date -d '1 hour ago' +%s)000 \
  --region $AWS_REGION
```

### CloudWatch Metrics

Monitor ECS service metrics:
- CPU Utilization
- Memory Utilization
- Task Count
- Health Check Status

Access metrics in AWS Console:
1. Navigate to CloudWatch > Metrics
2. Select ECS namespace
3. Filter by cluster and service name

### Health Checks

The application provides a health check endpoint:

```bash
# Check health via ALB
curl http://<alb-dns-name>/health

# Expected response:
{
  "status": "UP",
  "timestamp": "2024-01-01T00:00:00.0000000Z",
  "application": "BankManagementSystem",
  "version": "1.0.0"
}
```

### Setting Up Alarms

```bash
# Create CPU utilization alarm
aws cloudwatch put-metric-alarm \
  --alarm-name bankmanagement-high-cpu \
  --alarm-description "Alert when CPU exceeds 80%" \
  --metric-name CPUUtilization \
  --namespace AWS/ECS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --dimensions Name=ServiceName,Value=bankmanagement-system-service \
               Name=ClusterName,Value=bankmanagement-cluster
```

---

## Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptoms**: Tasks transition to STOPPED state immediately

**Possible Causes**:
- Invalid CPU/memory combination
- Image pull errors
- Missing IAM permissions
- Network configuration issues

**Solutions**:
```bash
# Check task stopped reason
aws ecs describe-tasks \
  --cluster bankmanagement-cluster \
  --tasks <task-id> \
  --region $AWS_REGION \
  --query 'tasks[0].stoppedReason'

# Check CloudWatch logs for errors
aws logs tail /ecs/bankmanagement-system --follow

# Verify IAM role permissions
aws iam get-role --role-name ecsTaskExecutionRole
```

#### 2. Health Check Failures

**Symptoms**: Tasks marked as unhealthy, continuous restarts

**Possible Causes**:
- Application not listening on correct port
- Health check endpoint not responding
- Insufficient startup time

**Solutions**:
```bash
# Increase health check grace period
aws ecs update-service \
  --cluster bankmanagement-cluster \
  --service bankmanagement-system-service \
  --health-check-grace-period-seconds 300

# Check application logs
aws logs tail /ecs/bankmanagement-system --follow

# Test health endpoint locally
docker run -p 8080:8080 <image-uri>
curl http://localhost:8080/health
```

#### 3. Database Connection Errors

**Symptoms**: Application logs show database connection failures

**Possible Causes**:
- Incorrect database credentials
- Network connectivity issues
- Security group rules blocking traffic

**Solutions**:
```bash
# Verify database credentials in Secrets Manager
aws secretsmanager get-secret-value \
  --secret-id bankmanagement/db-password

# Check security group rules
aws ec2 describe-security-groups \
  --group-ids <security-group-id>

# Test database connectivity from task
aws ecs execute-command \
  --cluster bankmanagement-cluster \
  --task <task-id> \
  --container bankmanagement-system \
  --interactive \
  --command "/bin/bash"
```

#### 4. Out of Memory Errors

**Symptoms**: Tasks killed with OOM errors

**Solutions**:
```bash
# Increase memory allocation in task definition
# Valid Fargate combinations:
# CPU: 512 → Memory: 1024, 2048, 3072, 4096
# CPU: 1024 → Memory: 2048-8192 (increments of 1024)

# Update task definition with higher memory
# Then update service
aws ecs update-service \
  --cluster bankmanagement-cluster \
  --service bankmanagement-system-service \
  --task-definition bankmanagement-system-task:new-revision \
  --force-new-deployment
```

#### 5. Image Pull Errors

**Symptoms**: "CannotPullContainerError" in task stopped reason

**Solutions**:
```bash
# Verify ECR repository exists
aws ecr describe-repositories \
  --repository-names bankmanagement-system

# Check ECR permissions
aws ecr get-repository-policy \
  --repository-name bankmanagement-system

# Verify task execution role has ECR permissions
aws iam list-attached-role-policies \
  --role-name ecsTaskExecutionRole
```

### Debugging Commands

```bash
# List running tasks
aws ecs list-tasks \
  --cluster bankmanagement-cluster \
  --service-name bankmanagement-system-service

# Describe task details
aws ecs describe-tasks \
  --cluster bankmanagement-cluster \
  --tasks <task-id>

# View service events
aws ecs describe-services \
  --cluster bankmanagement-cluster \
  --services bankmanagement-system-service \
  --query 'services[0].events'

# Check task definition
aws ecs describe-task-definition \
  --task-definition bankmanagement-system-task

# View container logs
aws logs get-log-events \
  --log-group-name /ecs/bankmanagement-system \
  --log-stream-name ecs/bankmanagement-system/<task-id>
```

---

## Security Considerations

### Container Security

1. **Non-Root User**: Application runs as non-root user `bankapp`
2. **Minimal Base Image**: Uses official Microsoft runtime image
3. **No Unnecessary Tools**: Runtime image doesn't include curl, wget, or other tools
4. **Read-Only Root Filesystem**: Consider enabling in task definition

### Network Security

1. **Security Groups**: Restrict inbound traffic to necessary ports only
2. **Private Subnets**: Consider deploying tasks in private subnets with NAT Gateway
3. **VPC Endpoints**: Use VPC endpoints for AWS services (ECR, CloudWatch, Secrets Manager)

### Secrets Management

1. **AWS Secrets Manager**: Store sensitive data (database passwords, API keys)
2. **IAM Roles**: Use task roles for AWS service access, not hardcoded credentials
3. **Environment Variables**: Never hardcode secrets in task definitions

### Best Practices

```bash
# Enable container insights for enhanced monitoring
aws ecs update-cluster-settings \
  --cluster bankmanagement-cluster \
  --settings name=containerInsights,value=enabled

# Enable encryption for CloudWatch logs
aws logs put-retention-policy \
  --log-group-name /ecs/bankmanagement-system \
  --retention-in-days 30

# Scan images for vulnerabilities
aws ecr start-image-scan \
  --repository-name bankmanagement-system \
  --image-id imageTag=latest
```

---

## Scaling and Performance

### Auto Scaling Configuration

#### Target Tracking Scaling

```bash
# Create auto scaling target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/bankmanagement-cluster/bankmanagement-system-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10

# Create CPU-based scaling policy
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/bankmanagement-cluster/bankmanagement-system-service \
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

# Create memory-based scaling policy
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/bankmanagement-cluster/bankmanagement-system-service \
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

#### .NET Runtime Optimization

1. **ReadyToRun Images**: Consider using ReadyToRun compilation for faster startup
2. **Tiered Compilation**: Enable for better runtime performance
3. **Garbage Collection**: Configure GC settings for containerized environments

```dockerfile
# Add to Dockerfile for performance optimization
ENV DOTNET_TieredCompilation=1 \
    DOTNET_ReadyToRun=1 \
    DOTNET_TC_QuickJitForLoops=1
```

#### Resource Allocation

**Recommended Fargate Configurations**:

| Workload | CPU | Memory | Use Case |
|----------|-----|--------|----------|
| Development | 256 | 512 MB | Testing, low traffic |
| Production (Small) | 512 | 1024 MB | Low to medium traffic |
| Production (Medium) | 1024 | 2048 MB | Medium to high traffic |
| Production (Large) | 2048 | 4096 MB | High traffic, complex operations |

### Blue/Green Deployments

```bash
# Create new task definition revision
aws ecs register-task-definition \
  --cli-input-json file://ecs/task-definition.json

# Update service with deployment configuration
aws ecs update-service \
  --cluster bankmanagement-cluster \
  --service bankmanagement-system-service \
  --task-definition bankmanagement-system-task:new-revision \
  --deployment-configuration '{
    "deploymentCircuitBreaker": {
      "enable": true,
      "rollback": true
    },
    "maximumPercent": 200,
    "minimumHealthyPercent": 100
  }'
```

### Load Balancer Optimization

```bash
# Configure ALB target group settings
aws elbv2 modify-target-group \
  --target-group-arn <target-group-arn> \
  --health-check-interval-seconds 30 \
  --health-check-timeout-seconds 5 \
  --healthy-threshold-count 2 \
  --unhealthy-threshold-count 3 \
  --deregistration-delay-timeout-seconds 30
```

---

## Additional Resources

### AWS Documentation
- [ECS Fargate Documentation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)
- [ECS Task Definitions](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_definitions.html)
- [ECS Service Auto Scaling](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/service-auto-scaling.html)

### .NET Documentation
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Containerize .NET Applications](https://docs.microsoft.com/en-us/dotnet/core/docker/introduction)
- [.NET Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/core/performance/)

### Docker Documentation
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Multi-stage Builds](https://docs.docker.com/develop/develop-images/multistage-build/)

---

## Support and Maintenance

### Regular Maintenance Tasks

1. **Update Base Images**: Regularly update .NET runtime images for security patches
2. **Review Logs**: Monitor CloudWatch logs for errors and warnings
3. **Cost Optimization**: Review resource utilization and adjust task sizes
4. **Security Scans**: Regularly scan images for vulnerabilities
5. **Backup Configuration**: Version control all infrastructure as code

### Updating the Application

```bash
# 1. Build new image with updated code
./scripts/build-push.sh

# 2. Deploy new version
./scripts/deploy-image.sh

# 3. Monitor deployment
aws ecs describe-services \
  --cluster bankmanagement-cluster \
  --services bankmanagement-system-service \
  --query 'services[0].events[0:5]'

# 4. Verify health
curl http://<alb-dns>/health
```

---

## Conclusion

This deployment guide provides comprehensive instructions for containerizing and deploying the BankManagementSystem application to AWS ECS Fargate. Follow the steps carefully, and refer to the troubleshooting section if you encounter any issues.

For additional support or questions, please refer to the AWS documentation or contact your DevOps team.
