@echo off
setlocal enabledelayedexpansion

REM Deploy BankDatabaseAccess to Azure AKS
REM This script deploys the application to Azure Kubernetes Service (AKS)

echo ========================================
echo BankDatabaseAccess - Deploy to Azure AKS
echo ========================================
echo.

REM Prompt for Azure configuration
echo Azure AKS Configuration
set /p RESOURCE_GROUP="Enter Azure resource group name: "
set /p CLUSTER_NAME="Enter AKS cluster name: "

REM Validate inputs
if "!RESOURCE_GROUP!"=="" (
    echo Error: Resource group is required.
    exit /b 1
)
if "!CLUSTER_NAME!"=="" (
    echo Error: Cluster name is required.
    exit /b 1
)

REM Prompt for Docker image URI
echo.
set /p IMAGE_URI="Enter Docker image URI (e.g., myregistry.azurecr.io/bankdatabaseaccess:latest): "

if "!IMAGE_URI!"=="" (
    echo Error: Docker image URI is required.
    exit /b 1
)

REM Prompt for environment-specific configuration
echo.
echo Application Configuration
set /p DATABASE_SERVER="Enter DATABASE_SERVER (or press Enter to skip): "
set /p DATABASE_NAME="Enter DATABASE_NAME (or press Enter to skip): "
set /p DATABASE_USER="Enter DATABASE_USER (or press Enter to skip): "
set /p DATABASE_PASSWORD="Enter DATABASE_PASSWORD (or press Enter to skip): "

REM Set default values if not provided
if "!DATABASE_SERVER!"=="" set DATABASE_SERVER=sqlserver
if "!DATABASE_NAME!"=="" set DATABASE_NAME=BankDB
if "!DATABASE_USER!"=="" set DATABASE_USER=sa

REM Configure kubectl to use AKS cluster
echo.
echo Configuring kubectl for AKS cluster...
az aks get-credentials --resource-group !RESOURCE_GROUP! --name !CLUSTER_NAME! --overwrite-existing

if !ERRORLEVEL! neq 0 (
    echo Failed to configure kubectl. Please check your Azure credentials and cluster details.
    exit /b 1
)

REM Verify cluster connectivity
echo.
echo Verifying cluster connectivity...
kubectl cluster-info

if !ERRORLEVEL! neq 0 (
    echo Failed to connect to cluster. Please check your configuration.
    exit /b 1
)

REM Update Kubernetes manifests with actual values
echo.
echo Updating Kubernetes manifests...

REM Create temporary directory for updated manifests
set TEMP_DIR=%TEMP%\k8s-deploy-%RANDOM%
mkdir !TEMP_DIR!
xcopy /E /I /Q kubernetes !TEMP_DIR! >nul

REM Replace placeholders in deployment.yaml using PowerShell
powershell -Command "(Get-Content '!TEMP_DIR!\deployment.yaml') -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content '!TEMP_DIR!\deployment.yaml'"
powershell -Command "(Get-Content '!TEMP_DIR!\deployment.yaml') -replace '{{DATABASE_SERVER}}', '!DATABASE_SERVER!' | Set-Content '!TEMP_DIR!\deployment.yaml'"
powershell -Command "(Get-Content '!TEMP_DIR!\deployment.yaml') -replace '{{DATABASE_NAME}}', '!DATABASE_NAME!' | Set-Content '!TEMP_DIR!\deployment.yaml'"
powershell -Command "(Get-Content '!TEMP_DIR!\deployment.yaml') -replace '{{DATABASE_USER}}', '!DATABASE_USER!' | Set-Content '!TEMP_DIR!\deployment.yaml'"

REM Create secret for database password if provided
if not "!DATABASE_PASSWORD!"=="" (
    echo Creating Kubernetes secret for database password...
    kubectl create secret generic bankdatabaseaccess-secrets --from-literal=database-password=!DATABASE_PASSWORD! --namespace=bankdatabaseaccess --dry-run=client -o yaml | kubectl apply -f -
)

REM Apply Kubernetes manifests
echo.
echo Applying Kubernetes manifests...

REM Apply namespace
echo Creating namespace...
kubectl apply -f !TEMP_DIR!\namespace.yaml

REM Apply deployment
echo Creating deployment...
kubectl apply -f !TEMP_DIR!\deployment.yaml

REM Apply service
echo Creating service...
kubectl apply -f !TEMP_DIR!\service.yaml

REM Apply ingress
echo Creating ingress...
kubectl apply -f !TEMP_DIR!\ingress.yaml

REM Clean up temporary directory
rmdir /S /Q !TEMP_DIR!

REM Wait for deployment to be ready
echo.
echo Waiting for deployment to be ready...
kubectl rollout status deployment/bankdatabaseaccess -n bankdatabaseaccess --timeout=5m

if !ERRORLEVEL! neq 0 (
    echo Deployment failed to become ready. Checking pod status...
    kubectl get pods -n bankdatabaseaccess
    kubectl describe pods -n bankdatabaseaccess
    exit /b 1
)

REM Verify deployment
echo.
echo Verifying deployment...
kubectl get pods,svc,ingress -n bankdatabaseaccess

REM Get ingress URL
echo.
echo ========================================
echo Deployment Completed Successfully!
echo ========================================
echo.

for /f "delims=" %%i in ('kubectl get ingress bankdatabaseaccess-ingress -n bankdatabaseaccess -o jsonpath^="{.status.loadBalancer.ingress[0].ip}" 2^>nul') do set INGRESS_IP=%%i

if not "!INGRESS_IP!"=="" (
    echo Application URL: http://!INGRESS_IP!
) else (
    echo Ingress IP is pending. Run the following command to check status:
    echo kubectl get ingress bankdatabaseaccess-ingress -n bankdatabaseaccess
)

echo.
echo Useful Commands:
echo   View pods:        kubectl get pods -n bankdatabaseaccess
echo   View logs:        kubectl logs -f deployment/bankdatabaseaccess -n bankdatabaseaccess
echo   View services:    kubectl get svc -n bankdatabaseaccess
echo   View ingress:     kubectl get ingress -n bankdatabaseaccess
echo   Scale deployment: kubectl scale deployment/bankdatabaseaccess --replicas=3 -n bankdatabaseaccess
echo   Delete deployment: kubectl delete namespace bankdatabaseaccess
echo.
