#!/bin/bash

# Deploy to AWS ECS Fargate Script for BankDatabaseAccess
# This script deploys the Docker image to AWS ECS Fargate

set -e
set -o pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}AWS ECS Fargate Deployment Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Project configuration
PROJECT_NAME="bankdatabaseaccess"
TASK_FAMILY="bankdatabaseaccess-task"
SERVICE_NAME="bankdatabaseaccess-service"

# Prompt for AWS configuration
echo -e "${YELLOW}=== AWS Configuration ===${NC}"
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS cluster name (e.g., my-ecs-cluster): " CLUSTER_NAME

# Get AWS Account ID
echo -e "${YELLOW}Retrieving AWS Account ID...${NC}"
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo -e "${GREEN}Account ID: $ACCOUNT_ID${NC}"

# Check if cluster exists, create if not
echo -e "${YELLOW}Checking ECS cluster...${NC}"
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo -e "${YELLOW}Creating ECS cluster: $CLUSTER_NAME${NC}"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}
echo -e "${GREEN}Cluster ready: $CLUSTER_NAME${NC}"

# Prompt for network configuration
echo ""
echo -e "${YELLOW}=== Network Configuration ===${NC}"
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_IDS
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

# Parse subnets
IFS=',' read -ra SUBNETS <<< "$SUBNET_IDS"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

# Prompt for Docker image
echo ""
echo -e "${YELLOW}=== Docker Image Configuration ===${NC}"
read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/app:latest): " IMAGE_URI

# Prompt for database configuration
echo ""
echo -e "${YELLOW}=== Database Configuration ===${NC}"
read -p "Enter database server (e.g., mydb.region.rds.amazonaws.com): " DB_SERVER
read -p "Enter database name: " DB_NAME
read -p "Enter database user: " DB_USER
read -sp "Enter database password: " DB_PASSWORD
echo ""

# Build connection string
DB_CONNECTION_STRING="Server=$DB_SERVER;Database=$DB_NAME;User Id=$DB_USER;Password=$DB_PASSWORD;TrustServerCertificate=True;"

# Prompt for load balancer
echo ""
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

if [[ "$NEED_LB" =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Creating Application Load Balancer...${NC}"
    
    # Create ALB
    ALB_NAME="${PROJECT_NAME}-alb"
    ALB_ARN=$(aws elbv2 create-load-balancer \
        --name "$ALB_NAME" \
        --subnets "$SUBNET_1" "$SUBNET_2" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$ALB_ARN" ]; then
        # ALB might already exist, try to get it
        ALB_ARN=$(aws elbv2 describe-load-balancers \
            --names "$ALB_NAME" \
            --region "$AWS_REGION" \
            --query 'LoadBalancers[0].LoadBalancerArn' \
            --output text 2>/dev/null || echo "")
    fi
    
    if [ -z "$ALB_ARN" ]; then
        echo -e "${RED}Failed to create or find load balancer${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Load Balancer ARN: $ALB_ARN${NC}"
    
    # Create Target Group with ip target type (required for Fargate)
    TG_NAME="${PROJECT_NAME}-tg"
    TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
        --name "$TG_NAME" \
        --protocol HTTP \
        --port 8080 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-enabled \
        --health-check-protocol HTTP \
        --health-check-path /health \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$TARGET_GROUP_ARN" ]; then
        # Target group might already exist, try to get it
        TARGET_GROUP_ARN=$(aws elbv2 describe-target-groups \
            --names "$TG_NAME" \
            --region "$AWS_REGION" \
            --query 'TargetGroups[0].TargetGroupArn' \
            --output text 2>/dev/null || echo "")
    fi
    
    if [ -z "$TARGET_GROUP_ARN" ]; then
        echo -e "${RED}Failed to create or find target group${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Target Group ARN: $TARGET_GROUP_ARN${NC}"
    
    # Create listener
    LISTENER_ARN=$(aws elbv2 create-listener \
        --load-balancer-arn "$ALB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" \
        --query 'Listeners[0].ListenerArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$LISTENER_ARN" ]; then
        echo -e "${YELLOW}Listener might already exist, continuing...${NC}"
    else
        echo -e "${GREEN}Listener created: $LISTENER_ARN${NC}"
    fi
    
    # Get ALB DNS name
    ALB_DNS=$(aws elbv2 describe-load-balancers \
        --load-balancer-arns "$ALB_ARN" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].DNSName' \
        --output text)
    
    echo -e "${GREEN}Load Balancer DNS: $ALB_DNS${NC}"
else
    TARGET_GROUP_ARN=""
    echo -e "${YELLOW}Skipping load balancer configuration${NC}"
fi

# Create CloudWatch log group
echo ""
echo -e "${YELLOW}Creating CloudWatch log group...${NC}"
aws logs create-log-group --log-group-name "/ecs/$PROJECT_NAME" --region "$AWS_REGION" 2>/dev/null || echo -e "${YELLOW}Log group already exists${NC}"

# Update task definition JSON
echo ""
echo -e "${YELLOW}Preparing task definition...${NC}"
cd "$(dirname "$0")/.."

# Create temporary task definition with replacements
cat ecs/task-definition.json | \
    sed "s|{{IMAGE_URI}}|$IMAGE_URI|g" | \
    sed "s|{{AWS_REGION}}|$AWS_REGION|g" | \
    sed "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" | \
    sed "s|{{DB_CONNECTION_STRING}}|$DB_CONNECTION_STRING|g" | \
    sed "s|{{DB_SERVER}}|$DB_SERVER|g" | \
    sed "s|{{DB_NAME}}|$DB_NAME|g" | \
    sed "s|{{DB_USER}}|$DB_USER|g" | \
    sed "s|{{DB_PASSWORD}}|$DB_PASSWORD|g" \
    > /tmp/task-definition-temp.json

# Register task definition
echo -e "${YELLOW}Registering task definition...${NC}"
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file:///tmp/task-definition-temp.json \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

if [ -z "$TASK_DEF_ARN" ]; then
    echo -e "${RED}Failed to register task definition${NC}"
    exit 1
fi

echo -e "${GREEN}Task definition registered: $TASK_DEF_ARN${NC}"

# Clean up temp file
rm -f /tmp/task-definition-temp.json

# Update service definition JSON
if [ -n "$TARGET_GROUP_ARN" ]; then
    # With load balancer
    cat ecs/service-definition.json | \
        sed "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" | \
        sed "s|{{SUBNET_1}}|$SUBNET_1|g" | \
        sed "s|{{SUBNET_2}}|$SUBNET_2|g" | \
        sed "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" | \
        sed "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" \
        > /tmp/service-definition-temp.json
else
    # Without load balancer - remove loadBalancers section
    cat ecs/service-definition.json | \
        sed "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" | \
        sed "s|{{SUBNET_1}}|$SUBNET_1|g" | \
        sed "s|{{SUBNET_2}}|$SUBNET_2|g" | \
        sed "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" | \
        jq 'del(.loadBalancers) | del(.healthCheckGracePeriodSeconds)' \
        > /tmp/service-definition-temp.json
fi

# Check if service exists
echo ""
echo -e "${YELLOW}Checking if service exists...${NC}"
SERVICE_EXISTS=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[?serviceName==`'"$SERVICE_NAME"'`].serviceName' \
    --output text 2>/dev/null || echo "")

if [ -z "$SERVICE_EXISTS" ] || [ "$SERVICE_EXISTS" == "None" ]; then
    # Create new service
    echo -e "${YELLOW}Creating new ECS service...${NC}"
    aws ecs create-service \
        --cli-input-json file:///tmp/service-definition-temp.json \
        --region "$AWS_REGION"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to create service${NC}"
        rm -f /tmp/service-definition-temp.json
        exit 1
    fi
    
    echo -e "${GREEN}Service created successfully!${NC}"
else
    # Update existing service
    echo -e "${YELLOW}Updating existing ECS service...${NC}"
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --desired-count 2 \
        --region "$AWS_REGION"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to update service${NC}"
        rm -f /tmp/service-definition-temp.json
        exit 1
    fi
    
    echo -e "${GREEN}Service updated successfully!${NC}"
fi

# Clean up temp file
rm -f /tmp/service-definition-temp.json

# Wait for service to stabilize
echo ""
echo -e "${YELLOW}Waiting for service to stabilize (this may take a few minutes)...${NC}"
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

# Verify deployment
echo ""
echo -e "${GREEN}=== Deployment Status ===${NC}"
aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].[serviceName,status,runningCount,desiredCount]' \
    --output table

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Completed Successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}Service Details:${NC}"
echo "  Cluster: $CLUSTER_NAME"
echo "  Service: $SERVICE_NAME"
echo "  Region: $AWS_REGION"
echo ""

if [ -n "$ALB_DNS" ]; then
    echo -e "${YELLOW}Application Access:${NC}"
    echo "  Load Balancer: http://$ALB_DNS"
    echo "  Health Check: http://$ALB_DNS/health"
    echo ""
fi

echo -e "${YELLOW}CloudWatch Logs:${NC}"
echo "  Log Group: /ecs/$PROJECT_NAME"
echo "  Region: $AWS_REGION"
echo ""
echo -e "${YELLOW}Useful Commands:${NC}"
echo "  View logs: aws logs tail /ecs/$PROJECT_NAME --follow --region $AWS_REGION"
echo "  List tasks: aws ecs list-tasks --cluster $CLUSTER_NAME --service-name $SERVICE_NAME --region $AWS_REGION"
echo "  Describe service: aws ecs describe-services --cluster $CLUSTER_NAME --services $SERVICE_NAME --region $AWS_REGION"
echo ""
