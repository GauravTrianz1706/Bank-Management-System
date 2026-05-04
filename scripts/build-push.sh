#!/bin/bash

# Build and Push Script for BankManagementSystem Docker Image
# Supports AWS ECR and Docker Hub registries

set -e
set -o pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}BankManagementSystem - Build & Push${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Project configuration
PROJECT_NAME="BankManagementSystem"

# Sanitize image name: lowercase, replace spaces/special chars with hyphens, trim hyphens
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo -e "${YELLOW}Select Registry Type:${NC}"
echo "1. AWS ECR (Elastic Container Registry)"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" == "1" ]; then
    echo ""
    echo -e "${GREEN}=== AWS ECR Configuration ===${NC}"
    
    # AWS ECR Configuration
    read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    read -p "Enter ECR Repository Name (default: $IMAGE_NAME): " ECR_REPO
    ECR_REPO=${ECR_REPO:-$IMAGE_NAME}
    
    # Construct registry URL
    REGISTRY_URL="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"
    
    echo ""
    echo -e "${YELLOW}Authenticating with AWS ECR...${NC}"
    
    # Login to ECR
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to authenticate with AWS ECR${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Successfully authenticated with AWS ECR${NC}"
    
    # Check if repository exists, create if it doesn't
    echo -e "${YELLOW}Checking ECR repository...${NC}"
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || {
        echo -e "${YELLOW}Repository does not exist. Creating ECR repository: $ECR_REPO${NC}"
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
        echo -e "${GREEN}ECR repository created successfully${NC}"
    }
    
    # Prompt for image tag
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="$REGISTRY_URL/$ECR_REPO:$IMAGE_TAG"
    
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    echo ""
    echo -e "${GREEN}=== Docker Hub Configuration ===${NC}"
    
    # Docker Hub Configuration
    read -p "Enter Docker Hub Username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub Password/Token: " DOCKER_PASSWORD
    echo ""
    
    echo -e "${YELLOW}Authenticating with Docker Hub...${NC}"
    
    # Login to Docker Hub
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to authenticate with Docker Hub${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Successfully authenticated with Docker Hub${NC}"
    
    # Prompt for image tag
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="$DOCKER_USERNAME/$IMAGE_NAME:$IMAGE_TAG"
    
else
    echo -e "${RED}Invalid choice. Exiting.${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}=== Building Docker Image ===${NC}"
echo -e "Image: ${YELLOW}$FULL_IMAGE_NAME${NC}"
echo ""

# Build Docker image from repository root
docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker build failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Docker image built successfully!${NC}"
echo ""

# Push image to registry
echo -e "${GREEN}=== Pushing Image to Registry ===${NC}"
docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker push failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Build and Push Completed Successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Image: ${YELLOW}$FULL_IMAGE_NAME${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Update ECS task definition with this image URI"
echo "2. Run deploy-image.sh to deploy to AWS ECS"
echo ""
