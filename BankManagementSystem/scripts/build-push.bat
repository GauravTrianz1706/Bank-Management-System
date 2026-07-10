@echo off
setlocal enabledelayedexpansion

REM Build and Push Script for BankManagementSystem
REM This script builds the Docker image and pushes it to a container registry

echo ==========================================
echo BankManagementSystem - Build and Push
echo ==========================================
echo.

set PROJECT_NAME=BankManagementSystem

REM Sanitize project name for Docker tag (lowercase, hyphenate)
set IMAGE_NAME=bankmanagementsystem

echo Select container registry:
echo 1. Azure Container Registry (ACR)
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "

if "!REGISTRY_CHOICE!"=="1" (
    REM Azure Container Registry
    echo.
    echo Azure Container Registry Configuration
    echo --------------------------------------
    set /p ACR_NAME="Enter ACR name (e.g., myregistry): "
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!ACR_NAME!.azurecr.io/!IMAGE_NAME!:!IMAGE_TAG!
    
    echo.
    echo Logging in to Azure Container Registry...
    az acr login --name !ACR_NAME!
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: ACR login failed. Please check your Azure CLI configuration.
        exit /b 1
    )
    
) else if "!REGISTRY_CHOICE!"=="2" (
    REM Docker Hub
    echo.
    echo Docker Hub Configuration
    echo ------------------------
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub password/token: "
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!
    
    echo.
    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Docker Hub login failed. Please check your credentials.
        exit /b 1
    )
    
) else (
    echo ERROR: Invalid choice. Please select 1 or 2.
    exit /b 1
)

echo.
echo Building Docker image: !FULL_IMAGE_NAME!
echo --------------------------------------

REM Build from repository root with correct context
docker build -f BankManagementSystem\Dockerfile -t !FULL_IMAGE_NAME! .

if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker build failed.
    exit /b 1
)

echo.
echo Pushing image to registry...
echo ----------------------------

docker push !FULL_IMAGE_NAME!

if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker push failed.
    exit /b 1
)

echo.
echo ==========================================
echo SUCCESS!
echo ==========================================
echo Image: !FULL_IMAGE_NAME!
echo.
echo Next steps:
echo 1. Update kubernetes\deployment.yaml with the image URI
echo 2. Run deploy-image.bat to deploy to Azure AKS
echo ==========================================

endlocal
