@echo off
setlocal enabledelayedexpansion

REM Build and Push Script for BankManagementSystem Docker Image
REM Supports AWS ECR and Docker Hub registries

echo ========================================
echo BankManagementSystem - Build ^& Push
echo ========================================
echo.

REM Project configuration
set PROJECT_NAME=BankManagementSystem

REM Sanitize image name: lowercase, replace spaces/special chars with hyphens
set IMAGE_NAME=bankmanagement-system

echo Select Registry Type:
echo 1. AWS ECR (Elastic Container Registry)
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "

if "!REGISTRY_CHOICE!"=="1" (
    echo.
    echo === AWS ECR Configuration ===
    
    REM AWS ECR Configuration
    set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS Account ID: "
    set /p ECR_REPO="Enter ECR Repository Name (default: !IMAGE_NAME!): "
    if "!ECR_REPO!"=="" set ECR_REPO=!IMAGE_NAME!
    
    REM Construct registry URL
    set REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com
    
    echo.
    echo Authenticating with AWS ECR...
    
    REM Login to ECR
    for /f "delims=" %%i in ('aws ecr get-login-password --region !AWS_REGION!') do set ECR_PASSWORD=%%i
    echo !ECR_PASSWORD! | docker login --username AWS --password-stdin !REGISTRY_URL!
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to authenticate with AWS ECR
        exit /b 1
    )
    
    echo Successfully authenticated with AWS ECR
    
    REM Check if repository exists, create if it doesn't
    echo Checking ECR repository...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Repository does not exist. Creating ECR repository: !ECR_REPO!
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
        if !ERRORLEVEL! neq 0 (
            echo Failed to create ECR repository
            exit /b 1
        )
        echo ECR repository created successfully
    )
    
    REM Prompt for image tag
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!
    
) else if "!REGISTRY_CHOICE!"=="2" (
    echo.
    echo === Docker Hub Configuration ===
    
    REM Docker Hub Configuration
    set /p DOCKER_USERNAME="Enter Docker Hub Username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub Password/Token: "
    
    echo Authenticating with Docker Hub...
    
    REM Login to Docker Hub
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to authenticate with Docker Hub
        exit /b 1
    )
    
    echo Successfully authenticated with Docker Hub
    
    REM Prompt for image tag
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!
    
) else (
    echo Invalid choice. Exiting.
    exit /b 1
)

echo.
echo === Building Docker Image ===
echo Image: !FULL_IMAGE_NAME!
echo.

REM Build Docker image from repository root
docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .

if !ERRORLEVEL! neq 0 (
    echo Docker build failed
    exit /b 1
)

echo.
echo Docker image built successfully!
echo.

REM Push image to registry
echo === Pushing Image to Registry ===
docker push "!FULL_IMAGE_NAME!"

if !ERRORLEVEL! neq 0 (
    echo Docker push failed
    exit /b 1
)

echo.
echo ========================================
echo Build and Push Completed Successfully!
echo ========================================
echo.
echo Image: !FULL_IMAGE_NAME!
echo.
echo Next Steps:
echo 1. Update ECS task definition with this image URI
echo 2. Run deploy-image.bat to deploy to AWS ECS
echo.

endlocal
