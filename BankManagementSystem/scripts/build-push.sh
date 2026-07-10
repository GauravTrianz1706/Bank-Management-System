#!/bin/bash
set -e
set -o pipefail

# Build and Push Script for BankManagementSystem
# This script builds the Docker image and pushes it to a container registry

echo "=========================================="
echo "BankManagementSystem - Build and Push"
echo "=========================================="
echo ""

PROJECT_NAME="BankManagementSystem"

# Sanitize project name for Docker tag (lowercase, hyphenate)
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "Select container registry:"
echo "1. Azure Container Registry (ACR)"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" == "1" ]; then
    # Azure Container Registry
    echo ""
    echo "Azure Container Registry Configuration"
    echo "--------------------------------------"
    read -p "Enter ACR name (e.g., myregistry): " ACR_NAME
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    
    FULL_IMAGE_NAME="${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"
    
    echo ""
    echo "Logging in to Azure Container Registry..."
    az acr login --name "$ACR_NAME"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: ACR login failed. Please check your Azure CLI configuration."
        exit 1
    fi
    
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    # Docker Hub
    echo ""
    echo "Docker Hub Configuration"
    echo "------------------------"
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub password/token: " DOCKER_PASSWORD
    echo ""
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    
    FULL_IMAGE_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}:${IMAGE_TAG}"
    
    echo ""
    echo "Logging in to Docker Hub..."
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Docker Hub login failed. Please check your credentials."
        exit 1
    fi
    
else
    echo "ERROR: Invalid choice. Please select 1 or 2."
    exit 1
fi

echo ""
echo "Building Docker image: $FULL_IMAGE_NAME"
echo "--------------------------------------"

# Build from repository root with correct context
docker build -f BankManagementSystem/Dockerfile -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo "ERROR: Docker build failed."
    exit 1
fi

echo ""
echo "Pushing image to registry..."
echo "----------------------------"

docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo "ERROR: Docker push failed."
    exit 1
fi

echo ""
echo "=========================================="
echo "SUCCESS!"
echo "=========================================="
echo "Image: $FULL_IMAGE_NAME"
echo ""
echo "Next steps:"
echo "1. Update kubernetes/deployment.yaml with the image URI"
echo "2. Run deploy-image.sh to deploy to Azure AKS"
echo "=========================================="
