#!/bin/bash

# Deploy Docker Image to AWS ECS Fargate
# This script deploys the containerized application to AWS ECS Fargate

set -e
set -o pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}AWS ECS Fargate Deployment Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Project configuration
PROJECT_NAME="bankmanagement-system"
SERVICE_NAME="${PROJECT_NAME}-service"
TASK_FAMILY="${PROJECT_NAME}-task"

# Prompt for AWS configuration
echo -e "${BLUE}AWS Configuration${NC}"
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS cluster name (e.g., my-ecs-cluster): " CLUSTER_NAME

# Get AWS Account ID
echo -e "${YELLOW}Retrieving AWS Account ID...${NC}"
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
if [ $? -ne 0 ]; then
    echo -e "${RED}Failed to retrieve AWS Account ID. Please check your AWS credentials.${NC}"
    exit 1
fi
echo -e "${GREEN}AWS Account ID: ${ACCOUNT_ID}${NC}"
echo ""

# Check if cluster exists, create if it doesn't
echo -e "${YELLOW}Checking if ECS cluster exists...${NC}"
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo -e "${YELLOW}Creating ECS cluster: ${CLUSTER_NAME}${NC}"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}
echo -e "${GREEN}ECS cluster ready: ${CLUSTER_NAME}${NC}"
echo ""

# Prompt for network configuration
echo -e "${BLUE}Network Configuration${NC}"
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_IDS
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

# Convert comma-separated subnets to array
IFS=',' read -ra SUBNET_ARRAY <<< "$SUBNET_IDS"
SUBNET_1="${SUBNET_ARRAY[0]}"
SUBNET_2="${SUBNET_ARRAY[1]:-$SUBNET_1}"

echo ""

# Prompt for Docker image URI
echo -e "${BLUE}Docker Image Configuration${NC}"
read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/app:latest): " IMAGE_URI
echo ""

# Ask about load balancer
echo -e "${BLUE}Load Balancer Configuration${NC}"
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

if [[ "$NEED_LB" =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Creating Application Load Balancer and Target Group...${NC}"
    
    # Create target group with target-type ip (required for Fargate awsvpc mode)
    TG_NAME="${PROJECT_NAME}-tg-$(date +%s)"
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
        --output text)
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to create target group${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Target Group created: ${TARGET_GROUP_ARN}${NC}"
    
    # Create Application Load Balancer
    ALB_NAME="${PROJECT_NAME}-alb"
    ALB_ARN=$(aws elbv2 create-load-balancer \
        --name "$ALB_NAME" \
        --subnets "${SUBNET_ARRAY[@]}" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text)
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to create load balancer${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Load Balancer created: ${ALB_ARN}${NC}"
    
    # Get ALB DNS name
    ALB_DNS=$(aws elbv2 describe-load-balancers \
        --load-balancer-arns "$ALB_ARN" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].DNSName' \
        --output text)
    
    # Create listener
    aws elbv2 create-listener \
        --load-balancer-arn "$ALB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" >/dev/null
    
    echo -e "${GREEN}Listener created for load balancer${NC}"
    echo ""
else
    echo -e "${YELLOW}Skipping load balancer creation${NC}"
    TARGET_GROUP_ARN=""
    echo ""
fi

# Create CloudWatch log group
echo -e "${YELLOW}Creating CloudWatch log group...${NC}"
aws logs create-log-group --log-group-name "/ecs/${PROJECT_NAME}" --region "$AWS_REGION" 2>/dev/null || true
echo -e "${GREEN}CloudWatch log group ready: /ecs/${PROJECT_NAME}${NC}"
echo ""

# Replace placeholders in task definition
echo -e "${YELLOW}Preparing task definition...${NC}"
TASK_DEF_FILE="ecs/task-definition.json"
TASK_DEF_TEMP="ecs/task-definition-temp.json"

sed "s|{{IMAGE_URI}}|${IMAGE_URI}|g; s|{{AWS_REGION}}|${AWS_REGION}|g; s|{{ACCOUNT_ID}}|${ACCOUNT_ID}|g" \
    "$TASK_DEF_FILE" > "$TASK_DEF_TEMP"

# Register task definition
echo -e "${YELLOW}Registering task definition...${NC}"
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://"$TASK_DEF_TEMP" \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

if [ $? -ne 0 ]; then
    echo -e "${RED}Failed to register task definition${NC}"
    rm -f "$TASK_DEF_TEMP"
    exit 1
fi

echo -e "${GREEN}Task definition registered: ${TASK_DEF_ARN}${NC}"
rm -f "$TASK_DEF_TEMP"
echo ""

# Prepare service definition
echo -e "${YELLOW}Preparing service definition...${NC}"
SERVICE_DEF_FILE="ecs/service-definition.json"
SERVICE_DEF_TEMP="ecs/service-definition-temp.json"

if [ -n "$TARGET_GROUP_ARN" ]; then
    # Include load balancer configuration
    sed "s|{{CLUSTER_NAME}}|${CLUSTER_NAME}|g; \
         s|{{SUBNET_1}}|${SUBNET_1}|g; \
         s|{{SUBNET_2}}|${SUBNET_2}|g; \
         s|{{SECURITY_GROUP}}|${SECURITY_GROUP}|g; \
         s|{{TARGET_GROUP_ARN}}|${TARGET_GROUP_ARN}|g" \
        "$SERVICE_DEF_FILE" > "$SERVICE_DEF_TEMP"
else
    # Remove load balancer section
    sed "s|{{CLUSTER_NAME}}|${CLUSTER_NAME}|g; \
         s|{{SUBNET_1}}|${SUBNET_1}|g; \
         s|{{SUBNET_2}}|${SUBNET_2}|g; \
         s|{{SECURITY_GROUP}}|${SECURITY_GROUP}|g" \
        "$SERVICE_DEF_FILE" | jq 'del(.loadBalancers, .healthCheckGracePeriodSeconds)' > "$SERVICE_DEF_TEMP"
fi

# Check if service exists
echo -e "${YELLOW}Checking if service exists...${NC}"
EXISTING_SERVICE=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[?status==`ACTIVE`].serviceName' \
    --output text 2>/dev/null)

if [ -n "$EXISTING_SERVICE" ] && [ "$EXISTING_SERVICE" != "None" ]; then
    echo -e "${YELLOW}Service exists. Updating service...${NC}"
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --region "$AWS_REGION" >/dev/null
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to update service${NC}"
        rm -f "$SERVICE_DEF_TEMP"
        exit 1
    fi
    
    echo -e "${GREEN}Service updated successfully${NC}"
else
    echo -e "${YELLOW}Creating new service...${NC}"
    aws ecs create-service \
        --cli-input-json file://"$SERVICE_DEF_TEMP" \
        --region "$AWS_REGION" >/dev/null
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to create service${NC}"
        rm -f "$SERVICE_DEF_TEMP"
        exit 1
    fi
    
    echo -e "${GREEN}Service created successfully${NC}"
fi

rm -f "$SERVICE_DEF_TEMP"
echo ""

# Wait for service to stabilize
echo -e "${YELLOW}Waiting for service to stabilize (this may take a few minutes)...${NC}"
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

if [ $? -ne 0 ]; then
    echo -e "${RED}Service failed to stabilize. Check ECS console for details.${NC}"
    exit 1
fi

echo -e "${GREEN}Service is stable!${NC}"
echo ""

# Display deployment information
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Successful!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${BLUE}Deployment Details:${NC}"
echo -e "  Cluster: ${CLUSTER_NAME}"
echo -e "  Service: ${SERVICE_NAME}"
echo -e "  Task Definition: ${TASK_DEF_ARN}"
echo -e "  Region: ${AWS_REGION}"
echo ""

if [ -n "$ALB_DNS" ]; then
    echo -e "${BLUE}Application Access:${NC}"
    echo -e "  Load Balancer DNS: http://${ALB_DNS}"
    echo -e "  Health Check: http://${ALB_DNS}/health"
    echo ""
fi

echo -e "${BLUE}CloudWatch Logs:${NC}"
echo -e "  Log Group: /ecs/${PROJECT_NAME}"
echo -e "  View logs: aws logs tail /ecs/${PROJECT_NAME} --follow --region ${AWS_REGION}"
echo ""

echo -e "${BLUE}Service Status:${NC}"
aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].[serviceName,status,runningCount,desiredCount]' \
    --output table

echo ""
echo -e "${GREEN}Deployment complete!${NC}"
