#!/bin/bash

# Deploy BankManagementSystem to AWS ECS Fargate
# This script deploys the containerized application to AWS ECS

set -e
set -o pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}BankManagementSystem - ECS Deployment${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Project configuration
PROJECT_NAME="bankmanagement-system"
TASK_FAMILY="bankmanagement-system-task"
SERVICE_NAME="bankmanagement-system-service"
LOG_GROUP="/ecs/bankmanagement-system"

# Prompt for AWS configuration
echo -e "${BLUE}=== AWS Configuration ===${NC}"
read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS Cluster Name (e.g., my-ecs-cluster): " CLUSTER_NAME

# Get AWS Account ID
echo -e "${YELLOW}Retrieving AWS Account ID...${NC}"
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo -e "${GREEN}Account ID: $ACCOUNT_ID${NC}"

# Check if cluster exists, create if it doesn't
echo -e "${YELLOW}Checking ECS cluster...${NC}"
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo -e "${YELLOW}Cluster does not exist. Creating ECS cluster: $CLUSTER_NAME${NC}"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
    echo -e "${GREEN}ECS cluster created successfully${NC}"
}

# Prompt for network configuration
echo ""
echo -e "${BLUE}=== Network Configuration ===${NC}"
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNETS_INPUT
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

# Parse subnets
IFS=',' read -ra SUBNET_ARRAY <<< "$SUBNETS_INPUT"
SUBNET_1="${SUBNET_ARRAY[0]}"
SUBNET_2="${SUBNET_ARRAY[1]:-$SUBNET_1}"

# Prompt for Docker image
echo ""
echo -e "${BLUE}=== Docker Image Configuration ===${NC}"
read -p "Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/bankmanagement-system:latest): " IMAGE_URI

# Prompt for database configuration
echo ""
echo -e "${BLUE}=== Database Configuration ===${NC}"
read -p "Enter Database Host: " DB_HOST
read -p "Enter Database Name (default: OpenBankLocal): " DB_NAME
DB_NAME=${DB_NAME:-OpenBankLocal}
read -p "Enter Database User: " DB_USER

# Prompt for load balancer
echo ""
echo -e "${BLUE}=== Load Balancer Configuration ===${NC}"
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
        echo -e "${YELLOW}Load balancer may already exist, attempting to retrieve...${NC}"
        ALB_ARN=$(aws elbv2 describe-load-balancers \
            --names "$ALB_NAME" \
            --region "$AWS_REGION" \
            --query 'LoadBalancers[0].LoadBalancerArn' \
            --output text 2>/dev/null || echo "")
    fi
    
    if [ -z "$ALB_ARN" ]; then
        echo -e "${RED}Failed to create or retrieve load balancer${NC}"
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
        echo -e "${YELLOW}Target group may already exist, attempting to retrieve...${NC}"
        TARGET_GROUP_ARN=$(aws elbv2 describe-target-groups \
            --names "$TG_NAME" \
            --region "$AWS_REGION" \
            --query 'TargetGroups[0].TargetGroupArn' \
            --output text 2>/dev/null || echo "")
    fi
    
    if [ -z "$TARGET_GROUP_ARN" ]; then
        echo -e "${RED}Failed to create or retrieve target group${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Target Group ARN: $TARGET_GROUP_ARN${NC}"
    
    # Create Listener
    LISTENER_ARN=$(aws elbv2 create-listener \
        --load-balancer-arn "$ALB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" \
        --query 'Listeners[0].ListenerArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$LISTENER_ARN" ]; then
        echo -e "${YELLOW}Listener may already exist${NC}"
    else
        echo -e "${GREEN}Listener created successfully${NC}"
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

# Create CloudWatch Log Group
echo ""
echo -e "${YELLOW}Creating CloudWatch Log Group...${NC}"
aws logs create-log-group --log-group-name "$LOG_GROUP" --region "$AWS_REGION" 2>/dev/null || echo -e "${YELLOW}Log group already exists${NC}"

# Update task definition JSON
echo ""
echo -e "${YELLOW}Preparing task definition...${NC}"
TASK_DEF_FILE="ecs/task-definition.json"

# Create temporary file with replacements
cp "$TASK_DEF_FILE" "${TASK_DEF_FILE}.tmp"

sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{AWS_REGION}}|$AWS_REGION|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{DB_HOST}}|$DB_HOST|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{DB_NAME}}|$DB_NAME|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{DB_USER}}|$DB_USER|g" "${TASK_DEF_FILE}.tmp"

# Register task definition
echo -e "${YELLOW}Registering task definition...${NC}"
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://"${TASK_DEF_FILE}.tmp" \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

if [ -z "$TASK_DEF_ARN" ]; then
    echo -e "${RED}Failed to register task definition${NC}"
    rm -f "${TASK_DEF_FILE}.tmp"
    exit 1
fi

echo -e "${GREEN}Task Definition registered: $TASK_DEF_ARN${NC}"

# Clean up temporary file
rm -f "${TASK_DEF_FILE}.tmp"

# Update service definition JSON
echo ""
echo -e "${YELLOW}Preparing service definition...${NC}"
SERVICE_DEF_FILE="ecs/service-definition.json"

# Create temporary file with replacements
cp "$SERVICE_DEF_FILE" "${SERVICE_DEF_FILE}.tmp"

sed -i "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SUBNET_1}}|$SUBNET_1|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SUBNET_2}}|$SUBNET_2|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" "${SERVICE_DEF_FILE}.tmp"

if [ -n "$TARGET_GROUP_ARN" ]; then
    sed -i "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" "${SERVICE_DEF_FILE}.tmp"
else
    # Remove loadBalancers section if no load balancer
    sed -i '/"loadBalancers":/,/],/d' "${SERVICE_DEF_FILE}.tmp"
    sed -i '/"healthCheckGracePeriodSeconds":/d' "${SERVICE_DEF_FILE}.tmp"
fi

# Check if service exists
echo -e "${YELLOW}Checking if service exists...${NC}"
EXISTING_SERVICE=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[?status==`ACTIVE`].serviceName' \
    --output text 2>/dev/null || echo "")

if [ -z "$EXISTING_SERVICE" ] || [ "$EXISTING_SERVICE" == "None" ]; then
    # Create new service
    echo -e "${YELLOW}Creating new ECS service...${NC}"
    aws ecs create-service \
        --cli-input-json file://"${SERVICE_DEF_FILE}.tmp" \
        --region "$AWS_REGION"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to create service${NC}"
        rm -f "${SERVICE_DEF_FILE}.tmp"
        exit 1
    fi
    
    echo -e "${GREEN}Service created successfully${NC}"
else
    # Update existing service
    echo -e "${YELLOW}Updating existing ECS service...${NC}"
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --force-new-deployment \
        --region "$AWS_REGION"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to update service${NC}"
        rm -f "${SERVICE_DEF_FILE}.tmp"
        exit 1
    fi
    
    echo -e "${GREEN}Service updated successfully${NC}"
fi

# Clean up temporary file
rm -f "${SERVICE_DEF_FILE}.tmp"

# Wait for service to stabilize
echo ""
echo -e "${YELLOW}Waiting for service to stabilize (this may take a few minutes)...${NC}"
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Service is stable${NC}"
else
    echo -e "${YELLOW}Service stabilization timed out, but deployment may still succeed${NC}"
fi

# Verify deployment
echo ""
echo -e "${YELLOW}Verifying deployment...${NC}"
RUNNING_COUNT=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].runningCount' \
    --output text)

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Completed Successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${BLUE}Deployment Details:${NC}"
echo -e "  Cluster: ${YELLOW}$CLUSTER_NAME${NC}"
echo -e "  Service: ${YELLOW}$SERVICE_NAME${NC}"
echo -e "  Task Definition: ${YELLOW}$TASK_DEF_ARN${NC}"
echo -e "  Running Tasks: ${YELLOW}$RUNNING_COUNT${NC}"
echo -e "  CloudWatch Logs: ${YELLOW}$LOG_GROUP${NC}"

if [ -n "$ALB_DNS" ]; then
    echo -e "  Load Balancer: ${YELLOW}http://$ALB_DNS${NC}"
    echo -e "  Health Check: ${YELLOW}http://$ALB_DNS/health${NC}"
fi

echo ""
echo -e "${BLUE}Useful Commands:${NC}"
echo -e "  View logs: ${YELLOW}aws logs tail $LOG_GROUP --follow --region $AWS_REGION${NC}"
echo -e "  Service status: ${YELLOW}aws ecs describe-services --cluster $CLUSTER_NAME --services $SERVICE_NAME --region $AWS_REGION${NC}"
echo -e "  List tasks: ${YELLOW}aws ecs list-tasks --cluster $CLUSTER_NAME --service-name $SERVICE_NAME --region $AWS_REGION${NC}"
echo ""
