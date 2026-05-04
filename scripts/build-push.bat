@echo off
setlocal enabledelayedexpansion

REM Build and Push Docker Image Script for BankManagementSystem
REM This script builds the Docker image and pushes it to a container registry

echo ========================================
echo Docker Build and Push Script
echo ========================================
echo.

REM Project configuration
set PROJECT_NAME=BankManagementSystem
set IMAGE_NAME=bankmanagementsystem

echo Project: %PROJECT_NAME%
echo Image Name: %IMAGE_NAME%
echo.

REM Prompt for registry selection
echo Select container registry:
echo 1. AWS ECR (Elastic Container Registry)
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter your choice (1 or 2): "

if "!REGISTRY_CHOICE!"=="1" (
    REM AWS ECR Configuration
    echo.
    echo AWS ECR Configuration
    set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS Account ID: "
    set /p ECR_REPO="Enter ECR Repository Name (default: %IMAGE_NAME%): "
    if "!ECR_REPO!"=="" set ECR_REPO=%IMAGE_NAME%
    
    set REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com
    
    echo.
    echo Authenticating with AWS ECR...
    aws ecr get-login-password --region !AWS_REGION! | docker login --username AWS --password-stdin !REGISTRY_URL!
    
    if !ERRORLEVEL! neq 0 (
        echo ECR authentication failed!
        exit /b 1
    )
    
    echo ECR authentication successful!
    
    REM Check if repository exists, create if it doesn't
    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository: !ECR_REPO!
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    )
    
    REM Prompt for image tag
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!
    
) else if "!REGISTRY_CHOICE!"=="2" (
    REM Docker Hub Configuration
    echo.
    echo Docker Hub Configuration
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub password or access token: "
    
    echo Authenticating with Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo Docker Hub authentication failed!
        exit /b 1
    )
    
    echo Docker Hub authentication successful!
    
    REM Prompt for image tag
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/%IMAGE_NAME%:!IMAGE_TAG!
    
) else (
    echo Invalid choice. Exiting.
    exit /b 1
)

echo.
echo ========================================
echo Building Docker Image
echo ========================================
echo Image: !FULL_IMAGE_NAME!
echo.

REM Build the Docker image
docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .

if !ERRORLEVEL! neq 0 (
    echo Docker build failed!
    exit /b 1
)

echo.
echo Docker build completed successfully!

REM Push the image
echo.
echo ========================================
echo Pushing Docker Image
echo ========================================
echo.

docker push "!FULL_IMAGE_NAME!"

if !ERRORLEVEL! neq 0 (
    echo Docker push failed!
    exit /b 1
)

echo.
echo ========================================
echo Success!
echo ========================================
echo Image pushed successfully: !FULL_IMAGE_NAME!
echo.
echo You can now deploy this image to your container platform.
echo.

endlocal
