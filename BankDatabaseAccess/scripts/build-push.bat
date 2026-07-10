@echo off
setlocal enabledelayedexpansion

REM Build and Push Script for BankDatabaseAccess
REM This script builds the Docker image and pushes it to a container registry
REM Supports Azure Container Registry (ACR) and Docker Hub

echo ========================================
echo BankDatabaseAccess - Build and Push
echo ========================================
echo.

REM Project configuration
set PROJECT_NAME=bankdatabaseaccess
set DOCKERFILE_PATH=BankDatabaseAccess\Dockerfile

REM Sanitize project name for Docker tag (lowercase, hyphenate)
set IMAGE_NAME=!PROJECT_NAME!
for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
    set IMAGE_NAME=!IMAGE_NAME:%%i=%%i!
)
set IMAGE_NAME=%IMAGE_NAME: =-%
set IMAGE_NAME=%IMAGE_NAME:_=-%
call :tolower IMAGE_NAME

echo Project: %PROJECT_NAME%
echo Image Name: !IMAGE_NAME!
echo.

REM Prompt for registry type
echo Select container registry:
echo 1. Azure Container Registry (ACR)
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "

if "!REGISTRY_CHOICE!"=="1" (
    REM Azure Container Registry
    echo.
    echo Azure Container Registry (ACR) Configuration
    set /p ACR_NAME="Enter ACR name (e.g., myregistry): "
    set /p ACR_RESOURCE_GROUP="Enter ACR resource group: "
    
    REM Login to ACR
    echo.
    echo Logging in to Azure Container Registry...
    az acr login --name !ACR_NAME!
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to login to ACR. Please check your credentials and try again.
        exit /b 1
    )
    
    REM Get ACR login server
    for /f "delims=" %%i in ('az acr show --name !ACR_NAME! --resource-group !ACR_RESOURCE_GROUP! --query loginServer --output tsv') do set ACR_LOGIN_SERVER=%%i
    
    REM Prompt for image tag
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    REM Sanitize tag
    set IMAGE_TAG=!IMAGE_TAG: =-!
    set IMAGE_TAG=!IMAGE_TAG:_=-!
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!ACR_LOGIN_SERVER!/!IMAGE_NAME!:!IMAGE_TAG!
    
) else if "!REGISTRY_CHOICE!"=="2" (
    REM Docker Hub
    echo.
    echo Docker Hub Configuration
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub password or access token: "
    
    REM Login to Docker Hub
    echo.
    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to login to Docker Hub. Please check your credentials and try again.
        exit /b 1
    )
    
    REM Prompt for image tag
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    REM Sanitize tag
    set IMAGE_TAG=!IMAGE_TAG: =-!
    set IMAGE_TAG=!IMAGE_TAG:_=-!
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!
    
) else (
    echo Invalid choice. Please run the script again and select 1 or 2.
    exit /b 1
)

echo.
echo Full Image Name: !FULL_IMAGE_NAME!
echo.

REM Build Docker image
echo Building Docker image...
docker build -f !DOCKERFILE_PATH! -t !FULL_IMAGE_NAME! .

if !ERRORLEVEL! neq 0 (
    echo Docker build failed. Please check the Dockerfile and try again.
    exit /b 1
)

echo Docker image built successfully!
echo.

REM Push Docker image
echo Pushing Docker image to registry...
docker push !FULL_IMAGE_NAME!

if !ERRORLEVEL! neq 0 (
    echo Docker push failed. Please check your registry credentials and try again.
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
echo 1. Update kubernetes\deployment.yaml with the image URI
echo 2. Run deploy-image.bat to deploy to Azure AKS
echo.

goto :eof

:tolower
for %%L in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
    set %1=!%1:%%L=%%L!
)
goto :eof
