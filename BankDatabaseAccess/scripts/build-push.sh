#!/bin/bash

# Build and Push Docker Image Script for BankDatabaseAccess
# This script builds the Docker image and pushes it to a container registry

set -e
set -o pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Docker Build and Push Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Project configuration
PROJECT_NAME="bankdatabaseaccess"

# Sanitize project name for Docker tag
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo -e "${YELLOW}Project:${NC} $IMAGE_NAME"
echo ""

# Prompt for registry type
echo -e "${YELLOW}Select Container Registry:${NC}"
echo "1. AWS ECR (Elastic Container Registry)"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" == "1" ]; then
    # AWS ECR Configuration
    echo ""
    echo -e "${GREEN}=== AWS ECR Configuration ===${NC}"
    read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    read -p "Enter ECR Repository Name (default: $IMAGE_NAME): " ECR_REPO
    ECR_REPO=${ECR_REPO:-$IMAGE_NAME}
    
    REGISTRY_URL="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"
    
    echo ""
    echo -e "${YELLOW}Authenticating with AWS ECR...${NC}"
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}ECR authentication failed!${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}ECR authentication successful!${NC}"
    
    # Check if repository exists, create if not
    echo -e "${YELLOW}Checking ECR repository...${NC}"
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || {
        echo -e "${YELLOW}Creating ECR repository: $ECR_REPO${NC}"
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
    }
    
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="$REGISTRY_URL/$ECR_REPO:$IMAGE_TAG"
    
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    # Docker Hub Configuration
    echo ""
    echo -e "${GREEN}=== Docker Hub Configuration ===${NC}"
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub password or access token: " DOCKER_PASSWORD
    echo ""
    
    echo -e "${YELLOW}Authenticating with Docker Hub...${NC}"
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Docker Hub authentication failed!${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Docker Hub authentication successful!${NC}"
    
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
echo -e "${YELLOW}Image:${NC} $FULL_IMAGE_NAME"
echo ""

# Build Docker image from repository root
cd "$(dirname "$0")/../.."
docker build -f BankDatabaseAccess/Dockerfile -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker build failed!${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Docker build successful!${NC}"

# Push image
echo ""
echo -e "${GREEN}=== Pushing Docker Image ===${NC}"
docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker push failed!${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Build and Push Completed Successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}Image:${NC} $FULL_IMAGE_NAME"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Update ECS task definition with the image URI"
echo "2. Run the deployment script: ./scripts/deploy-image.sh"
echo ""
