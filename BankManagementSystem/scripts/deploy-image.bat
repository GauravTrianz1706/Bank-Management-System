@echo off
setlocal enabledelayedexpansion

REM Deploy to Azure AKS Script for BankManagementSystem
REM This script deploys the application to Azure Kubernetes Service

echo ==========================================
echo BankManagementSystem - Deploy to Azure AKS
echo ==========================================
echo.

REM Prompt for Azure configuration
echo Azure AKS Configuration
echo ----------------------
set /p RESOURCE_GROUP="Enter Azure Resource Group name: "
set /p CLUSTER_NAME="Enter AKS Cluster name: "
set /p IMAGE_URI="Enter Docker image URI (e.g., myregistry.azurecr.io/bankmanagementsystem:latest): "

if "!RESOURCE_GROUP!"=="" (
    echo ERROR: Resource Group is required.
    exit /b 1
)
if "!CLUSTER_NAME!"=="" (
    echo ERROR: Cluster name is required.
    exit /b 1
)
if "!IMAGE_URI!"=="" (
    echo ERROR: Image URI is required.
    exit /b 1
)

REM Prompt for environment variables
echo.
echo Application Configuration (Optional)
echo -----------------------------------
set /p DATABASE_CONNECTION_STRING="Enter DATABASE_CONNECTION_STRING (or press Enter to skip): "

REM Configure kubectl
echo.
echo Configuring kubectl for AKS cluster...
echo --------------------------------------
az aks get-credentials --resource-group !RESOURCE_GROUP! --name !CLUSTER_NAME! --overwrite-existing

if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to configure kubectl. Please check your Azure credentials and cluster details.
    exit /b 1
)

REM Verify cluster connectivity
echo.
echo Verifying cluster connectivity...
kubectl cluster-info

if !ERRORLEVEL! neq 0 (
    echo ERROR: Cannot connect to Kubernetes cluster.
    exit /b 1
)

REM Update manifests with actual values
echo.
echo Updating Kubernetes manifests...
echo --------------------------------

REM Create temporary directory for updated manifests
set TEMP_DIR=%TEMP%\k8s-deploy-%RANDOM%
mkdir !TEMP_DIR!
xcopy /E /I /Q kubernetes !TEMP_DIR! >nul

REM Replace placeholders using PowerShell
powershell -Command "(Get-Content '!TEMP_DIR!\deployment.yaml') -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content '!TEMP_DIR!\deployment.yaml'"

if not "!DATABASE_CONNECTION_STRING!"=="" (
    powershell -Command "(Get-Content '!TEMP_DIR!\deployment.yaml') -replace '{{DATABASE_CONNECTION_STRING}}', '!DATABASE_CONNECTION_STRING!' | Set-Content '!TEMP_DIR!\deployment.yaml'"
) else (
    powershell -Command "(Get-Content '!TEMP_DIR!\deployment.yaml') | Where-Object { $_ -notmatch '{{DATABASE_CONNECTION_STRING}}' } | Set-Content '!TEMP_DIR!\deployment.yaml'"
)

REM Apply Kubernetes manifests
echo.
echo Deploying to Azure AKS...
echo -------------------------

echo Creating namespace...
kubectl apply -f !TEMP_DIR!\namespace.yaml

echo Deploying application...
kubectl apply -f !TEMP_DIR!\deployment.yaml

echo Creating service...
kubectl apply -f !TEMP_DIR!\service.yaml

echo Creating ingress...
kubectl apply -f !TEMP_DIR!\ingress.yaml

REM Wait for deployment rollout
echo.
echo Waiting for deployment to complete...
echo -------------------------------------
kubectl rollout status deployment/bankmanagementsystem -n bankmanagementsystem --timeout=5m

if !ERRORLEVEL! neq 0 (
    echo WARNING: Deployment rollout did not complete successfully.
    echo Check the status with: kubectl get pods -n bankmanagementsystem
)

REM Verify deployment
echo.
echo Verifying deployment...
echo -----------------------
kubectl get pods,svc,ingress -n bankmanagementsystem

REM Display completion message
echo.
echo ==========================================
echo DEPLOYMENT COMPLETE!
echo ==========================================
echo.
echo Application deployed to namespace: bankmanagementsystem
echo.
echo To check application status:
echo   kubectl get pods -n bankmanagementsystem
echo.
echo To view logs:
echo   kubectl logs -f deployment/bankmanagementsystem -n bankmanagementsystem
echo.
echo To access the application:
echo   kubectl port-forward -n bankmanagementsystem svc/bankmanagementsystem-service 8080:80
echo   Then open: http://localhost:8080/health
echo.
echo Ingress host: bankmanagementsystem.example.com
echo Note: Update your DNS or hosts file to point to the ingress IP
echo ==========================================

REM Cleanup
rmdir /S /Q !TEMP_DIR!

endlocal
