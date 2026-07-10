#!/bin/bash

# Build and Push Script for BankDatabaseAccess
# This script builds the Docker image and pushes it to a container registry
# Supports Azure Container Registry (ACR) and Docker Hub

set -e
set -o pipefail

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}BankDatabaseAccess - Build and Push${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Project configuration
PROJECT_NAME="bankdatabaseaccess"
DOCKERFILE_PATH="BankDatabaseAccess/Dockerfile"

# Sanitize project name for Docker tag (lowercase, hyphenate)
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo -e "${YELLOW}Project:${NC} $PROJECT_NAME"
echo -e "${YELLOW}Image Name:${NC} $IMAGE_NAME"
echo ""

# Prompt for registry type
echo "Select container registry:"
echo "1. Azure Container Registry (ACR)"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" == "1" ]; then
    # Azure Container Registry
    echo ""
    echo -e "${GREEN}Azure Container Registry (ACR) Configuration${NC}"
    read -p "Enter ACR name (e.g., myregistry): " ACR_NAME
    read -p "Enter ACR resource group: " ACR_RESOURCE_GROUP
    
    # Login to ACR
    echo ""
    echo -e "${YELLOW}Logging in to Azure Container Registry...${NC}"
    az acr login --name "$ACR_NAME"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to login to ACR. Please check your credentials and try again.${NC}"
        exit 1
    fi
    
    # Get ACR login server
    ACR_LOGIN_SERVER=$(az acr show --name "$ACR_NAME" --resource-group "$ACR_RESOURCE_GROUP" --query loginServer --output tsv)
    
    # Prompt for image tag
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="$ACR_LOGIN_SERVER/$IMAGE_NAME:$IMAGE_TAG"
    
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    # Docker Hub
    echo ""
    echo -e "${GREEN}Docker Hub Configuration${NC}"
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub password or access token: " DOCKER_PASSWORD
    echo ""
    
    # Login to Docker Hub
    echo ""
    echo -e "${YELLOW}Logging in to Docker Hub...${NC}"
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to login to Docker Hub. Please check your credentials and try again.${NC}"
        exit 1
    fi
    
    # Prompt for image tag
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="$DOCKER_USERNAME/$IMAGE_NAME:$IMAGE_TAG"
    
else
    echo -e "${RED}Invalid choice. Please run the script again and select 1 or 2.${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Full Image Name:${NC} $FULL_IMAGE_NAME"
echo ""

# Build Docker image
echo -e "${GREEN}Building Docker image...${NC}"
docker build -f "$DOCKERFILE_PATH" -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker build failed. Please check the Dockerfile and try again.${NC}"
    exit 1
fi

echo -e "${GREEN}Docker image built successfully!${NC}"
echo ""

# Push Docker image
echo -e "${GREEN}Pushing Docker image to registry...${NC}"
docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker push failed. Please check your registry credentials and try again.${NC}"
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
echo "1. Update kubernetes/deployment.yaml with the image URI"
echo "2. Run deploy-image.sh to deploy to Azure AKS"
echo ""
