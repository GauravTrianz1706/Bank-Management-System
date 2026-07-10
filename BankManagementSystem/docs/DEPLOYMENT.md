# BankManagementSystem - Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Architecture](#project-architecture)
4. [Local Development Setup](#local-development-setup)
5. [Docker Deployment](#docker-deployment)
6. [Azure AKS Deployment](#azure-aks-deployment)
7. [Configuration Management](#configuration-management)
8. [Troubleshooting](#troubleshooting)
9. [Security Considerations](#security-considerations)
10. [Technology-Specific Notes](#technology-specific-notes)

---

## Overview

BankManagementSystem is a .NET 6 Windows Forms application that has been containerized for cloud deployment. The application includes a built-in health check endpoint for container orchestration and monitoring.

**Key Features:**
- .NET 6 Windows Forms application
- Built-in HTTP health check endpoint (port 8080)
- Containerized for Azure AKS deployment
- Database connectivity support
- Production-ready configuration

**Technology Stack:**
- Framework: .NET 6 (net6.0-windows)
- Application Type: Windows Forms Desktop Application
- Dependencies: Newtonsoft.Json, log4net, HtmlAgilityPack
- Health Check Port: 8080
- Base Image: Regis/Repo/Test (explicit)

---

## Prerequisites

### Required Software

#### For Local Development:
- **.NET 6 SDK** or later
  - Download: https://dotnet.microsoft.com/download/dotnet/6.0
  - Verify: `dotnet --version`
- **Docker Desktop** (Windows/Mac) or Docker Engine (Linux)
  - Download: https://www.docker.com/products/docker-desktop
  - Verify: `docker --version`
- **Git** for version control
  - Download: https://git-scm.com/downloads

#### For Azure AKS Deployment:
- **Azure CLI** (az)
  - Download: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
  - Verify: `az --version`
  - Login: `az login`
- **kubectl** (Kubernetes CLI)
  - Download: https://kubernetes.io/docs/tasks/tools/
  - Verify: `kubectl version --client`
- **Active Azure Subscription**
  - Create: https://azure.microsoft.com/free/

### Azure Resources Required:
- Azure Container Registry (ACR) or Docker Hub account
- Azure Kubernetes Service (AKS) cluster
- Azure Resource Group
- Appropriate RBAC permissions

---

## Project Architecture

### Directory Structure
```
BankManagementSystem/
├── BankManagementSystem/          # Main application project
│   ├── *.cs                       # C# source files
│   ├── BankManagementSystem.csproj
│   ├── Program.cs                 # Application entry point
│   ├── HealthCheckEndpoint.cs     # Health check implementation
│   ├── Dockerfile                 # Container build instructions
│   ├── docker-compose.yml         # Local Docker orchestration
│   ├── .dockerignore              # Docker build exclusions
│   ├── kubernetes/                # Kubernetes manifests
│   │   ├── namespace.yaml
│   │   ├── deployment.yaml
│   │   ├── service.yaml
│   │   └── ingress.yaml
│   ├── scripts/                   # Deployment scripts
│   │   ├── build-push.sh
│   │   ├── build-push.bat
│   │   ├── deploy-image.sh
│   │   └── deploy-image.bat
│   └── docs/
│       └── DEPLOYMENT.md          # This file
└── BankDatabaseAccess/            # Database access layer
    └── BankDatabaseAccess.csproj
```

### Application Components

**Health Check Endpoint:**
- Port: 8080 (configurable via HEALTH_CHECK_PORT)
- Endpoints: `/health`, `/healthz`
- Response Format: JSON with status, timestamp, application name, version
- Used by: Kubernetes liveness/readiness probes

**Environment Variables:**
- `ASPNETCORE_ENVIRONMENT`: Production/Development
- `HEALTH_CHECK_PORT`: Health check listener port (default: 8080)
- `STARTUP_USER`: Container user identifier
- `DATABASE_CONNECTION_STRING`: Database connection string

---

## Local Development Setup

### Step 1: Clone the Repository
```bash
git clone <repository-url>
cd Backend/BankManagementSystem
```

### Step 2: Build the Application Locally
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build -c Release

# Run the application (requires Windows with GUI)
dotnet run --project BankManagementSystem.csproj
```

### Step 3: Test Health Check Endpoint
```bash
# Start the application
dotnet run

# In another terminal, test the health endpoint
curl http://localhost:8080/health
```

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00.0000000Z",
  "application": "BankManagementSystem",
  "version": "1.0.0"
}
```

---

## Docker Deployment

### Step 1: Build Docker Image Locally

#### Using Docker Compose (Recommended for Local Testing):
```bash
# From the Backend directory (parent of BankManagementSystem)
cd /path/to/Backend

# Build and start the container
docker-compose -f BankManagementSystem/docker-compose.yml up --build

# Test the health endpoint
curl http://localhost:8080/health
```

#### Using Docker CLI:
```bash
# From the Backend directory
cd /path/to/Backend

# Build the image
docker build -f BankManagementSystem/Dockerfile -t bankmanagementsystem:latest .

# Run the container
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DATABASE_CONNECTION_STRING="Server=host.docker.internal;Database=BankDB;User Id=sa;Password=YourPassword123;" \
  --name bankmanagementsystem \
  bankmanagementsystem:latest

# Check logs
docker logs -f bankmanagementsystem

# Test health endpoint
curl http://localhost:8080/health
```

### Step 2: Push to Container Registry

#### Option A: Using build-push.sh (Linux/Mac)
```bash
cd BankManagementSystem
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

Follow the prompts:
1. Select registry type (ACR or Docker Hub)
2. Enter registry credentials
3. Enter image tag (default: latest)

#### Option B: Using build-push.bat (Windows)
```cmd
cd BankManagementSystem
scripts\build-push.bat
```

Follow the prompts as above.

#### Manual Push to Azure ACR:
```bash
# Login to ACR
az acr login --name <your-acr-name>

# Tag the image
docker tag bankmanagementsystem:latest <your-acr-name>.azurecr.io/bankmanagementsystem:v1.0

# Push to ACR
docker push <your-acr-name>.azurecr.io/bankmanagementsystem:v1.0
```

---

## Azure AKS Deployment

### Prerequisites Checklist
- [ ] Azure CLI installed and logged in (`az login`)
- [ ] kubectl installed
- [ ] AKS cluster created and running
- [ ] Docker image pushed to ACR or Docker Hub
- [ ] ACR integrated with AKS (if using ACR)

### Step 1: Create AKS Cluster (if not exists)

```bash
# Set variables
RESOURCE_GROUP="rg-bankmanagementsystem"
LOCATION="eastus"
CLUSTER_NAME="aks-bankmanagementsystem"
ACR_NAME="<your-acr-name>"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create AKS cluster
az aks create \
  --resource-group $RESOURCE_GROUP \
  --name $CLUSTER_NAME \
  --node-count 2 \
  --node-vm-size Standard_D2s_v3 \
  --enable-managed-identity \
  --generate-ssh-keys

# Attach ACR to AKS (if using ACR)
az aks update \
  --resource-group $RESOURCE_GROUP \
  --name $CLUSTER_NAME \
  --attach-acr $ACR_NAME

# Get credentials
az aks get-credentials \
  --resource-group $RESOURCE_GROUP \
  --name $CLUSTER_NAME \
  --overwrite-existing
```

### Step 2: Deploy Using Automated Script

#### Option A: Using deploy-image.sh (Linux/Mac)
```bash
cd BankManagementSystem
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

Follow the prompts:
1. Enter Azure Resource Group name
2. Enter AKS Cluster name
3. Enter Docker image URI (e.g., `myregistry.azurecr.io/bankmanagementsystem:v1.0`)
4. Enter DATABASE_CONNECTION_STRING (optional)

#### Option B: Using deploy-image.bat (Windows)
```cmd
cd BankManagementSystem
scripts\deploy-image.bat
```

Follow the prompts as above.

### Step 3: Manual Deployment (Alternative)

```bash
# Configure kubectl
az aks get-credentials --resource-group $RESOURCE_GROUP --name $CLUSTER_NAME

# Update deployment.yaml with your image URI
sed -i 's|{{IMAGE_URI}}|<your-acr-name>.azurecr.io/bankmanagementsystem:v1.0|g' kubernetes/deployment.yaml

# Update environment variables
sed -i 's|{{DATABASE_CONNECTION_STRING}}|Server=sqlserver;Database=BankDB;User Id=sa;Password=YourPassword123;|g' kubernetes/deployment.yaml

# Apply manifests
kubectl apply -f kubernetes/namespace.yaml
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# Wait for deployment
kubectl rollout status deployment/bankmanagementsystem -n bankmanagementsystem

# Verify deployment
kubectl get pods,svc,ingress -n bankmanagementsystem
```

### Step 4: Verify Deployment

```bash
# Check pod status
kubectl get pods -n bankmanagementsystem

# View logs
kubectl logs -f deployment/bankmanagementsystem -n bankmanagementsystem

# Test health endpoint via port-forward
kubectl port-forward -n bankmanagementsystem svc/bankmanagementsystem-service 8080:80

# In another terminal
curl http://localhost:8080/health
```

### Step 5: Access the Application

#### Via Port Forward (Development):
```bash
kubectl port-forward -n bankmanagementsystem svc/bankmanagementsystem-service 8080:80
# Access: http://localhost:8080
```

#### Via Ingress (Production):
1. Get the ingress IP:
   ```bash
   kubectl get ingress -n bankmanagementsystem
   ```

2. Update DNS or `/etc/hosts`:
   ```
   <INGRESS_IP> bankmanagementsystem.example.com
   ```

3. Access: http://bankmanagementsystem.example.com

---

## Configuration Management

### Environment Variables

The application supports the following environment variables:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | Production | No |
| `HEALTH_CHECK_PORT` | Health check listener port | 8080 | No |
| `STARTUP_USER` | Container user identifier | containeruser | No |
| `DATABASE_CONNECTION_STRING` | Database connection string | - | Yes |

### Kubernetes ConfigMap (Optional)

Create a ConfigMap for non-sensitive configuration:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: bankmanagementsystem-config
  namespace: bankmanagementsystem
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  HEALTH_CHECK_PORT: "8080"
```

Apply:
```bash
kubectl apply -f configmap.yaml
```

Update deployment.yaml to use ConfigMap:
```yaml
envFrom:
- configMapRef:
    name: bankmanagementsystem-config
```

### Kubernetes Secrets (Recommended for Sensitive Data)

Create a secret for database credentials:

```bash
kubectl create secret generic bankmanagementsystem-secrets \
  --from-literal=DATABASE_CONNECTION_STRING="Server=sqlserver;Database=BankDB;User Id=sa;Password=YourPassword123;" \
  -n bankmanagementsystem
```

Update deployment.yaml to use Secret:
```yaml
env:
- name: DATABASE_CONNECTION_STRING
  valueFrom:
    secretKeyRef:
      name: bankmanagementsystem-secrets
      key: DATABASE_CONNECTION_STRING
```

---

## Troubleshooting

### Common Issues and Solutions

#### 1. Pod Not Starting (CrashLoopBackOff)

**Symptoms:**
```bash
kubectl get pods -n bankmanagementsystem
# NAME                                    READY   STATUS             RESTARTS
# bankmanagementsystem-xxx-yyy            0/1     CrashLoopBackOff   5
```

**Diagnosis:**
```bash
# Check pod logs
kubectl logs bankmanagementsystem-xxx-yyy -n bankmanagementsystem

# Describe pod for events
kubectl describe pod bankmanagementsystem-xxx-yyy -n bankmanagementsystem
```

**Common Causes:**
- Missing or invalid DATABASE_CONNECTION_STRING
- Insufficient resources (CPU/Memory)
- Base image compatibility issues
- Application startup errors

**Solutions:**
- Verify environment variables are set correctly
- Check resource limits in deployment.yaml
- Review application logs for startup errors
- Ensure database is accessible from AKS

#### 2. Health Check Failures

**Symptoms:**
```bash
kubectl describe pod <pod-name> -n bankmanagementsystem
# Events:
#   Liveness probe failed: HTTP probe failed with statuscode: 500
```

**Diagnosis:**
```bash
# Port-forward and test health endpoint
kubectl port-forward <pod-name> -n bankmanagementsystem 8080:8080
curl http://localhost:8080/health
```

**Solutions:**
- Verify HEALTH_CHECK_PORT environment variable
- Check if port 8080 is exposed in Dockerfile
- Ensure HealthCheckEndpoint is starting correctly
- Review application logs for health check errors

#### 3. Image Pull Errors

**Symptoms:**
```bash
kubectl get pods -n bankmanagementsystem
# NAME                                    READY   STATUS         RESTARTS
# bankmanagementsystem-xxx-yyy            0/1     ErrImagePull   0
```

**Solutions:**
- Verify image URI is correct in deployment.yaml
- Ensure ACR is attached to AKS: `az aks update --attach-acr <acr-name>`
- Check image exists: `az acr repository show --name <acr-name> --image bankmanagementsystem:v1.0`
- For Docker Hub, create image pull secret

#### 4. Service Not Accessible

**Symptoms:**
- Cannot access application via service or ingress

**Diagnosis:**
```bash
# Check service endpoints
kubectl get endpoints -n bankmanagementsystem

# Check service
kubectl describe svc bankmanagementsystem-service -n bankmanagementsystem

# Check ingress
kubectl describe ingress bankmanagementsystem-ingress -n bankmanagementsystem
```

**Solutions:**
- Verify pods are running and ready
- Check service selector matches pod labels
- Ensure ingress controller is installed
- Verify ingress annotations for Azure Application Gateway

#### 5. Database Connection Issues

**Symptoms:**
- Application starts but cannot connect to database

**Diagnosis:**
```bash
# Check logs for connection errors
kubectl logs -f deployment/bankmanagementsystem -n bankmanagementsystem | grep -i "database\|connection"
```

**Solutions:**
- Verify DATABASE_CONNECTION_STRING is correct
- Ensure database is accessible from AKS (network rules, firewall)
- Check database credentials
- Test connectivity from a debug pod:
  ```bash
  kubectl run -it --rm debug --image=mcr.microsoft.com/dotnet/runtime:6.0 --restart=Never -n bankmanagementsystem -- bash
  ```

### Debugging Commands

```bash
# Get all resources in namespace
kubectl get all -n bankmanagementsystem

# Describe deployment
kubectl describe deployment bankmanagementsystem -n bankmanagementsystem

# Get pod logs (current)
kubectl logs -f deployment/bankmanagementsystem -n bankmanagementsystem

# Get pod logs (previous container)
kubectl logs <pod-name> -n bankmanagementsystem --previous

# Execute command in pod
kubectl exec -it <pod-name> -n bankmanagementsystem -- /bin/bash

# Get events
kubectl get events -n bankmanagementsystem --sort-by='.lastTimestamp'

# Check resource usage
kubectl top pods -n bankmanagementsystem
kubectl top nodes
```

---

## Security Considerations

### 1. Container Security

**Non-Root User:**
- Dockerfile creates and uses non-root user `bankapp`
- Reduces attack surface and follows security best practices

**Image Scanning:**
```bash
# Scan image for vulnerabilities (using Azure Defender)
az acr task run --registry <acr-name> --cmd "scan <image-name>:<tag>"
```

### 2. Secrets Management

**Best Practices:**
- Never commit secrets to source control
- Use Kubernetes Secrets for sensitive data
- Consider Azure Key Vault for production:
  ```bash
  # Install CSI driver
  helm repo add csi-secrets-store-provider-azure https://azure.github.io/secrets-store-csi-driver-provider-azure/charts
  helm install csi csi-secrets-store-provider-azure/csi-secrets-store-provider-azure
  ```

### 3. Network Security

**Network Policies:**
```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: bankmanagementsystem-netpol
  namespace: bankmanagementsystem
spec:
  podSelector:
    matchLabels:
      app: bankmanagementsystem
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 8080
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 1433  # SQL Server
```

### 4. RBAC (Role-Based Access Control)

```bash
# Create service account
kubectl create serviceaccount bankmanagementsystem-sa -n bankmanagementsystem

# Create role
kubectl create role bankmanagementsystem-role \
  --verb=get,list,watch \
  --resource=pods,services \
  -n bankmanagementsystem

# Create role binding
kubectl create rolebinding bankmanagementsystem-rolebinding \
  --role=bankmanagementsystem-role \
  --serviceaccount=bankmanagementsystem:bankmanagementsystem-sa \
  -n bankmanagementsystem
```

### 5. Pod Security Standards

Update deployment.yaml with security context:
```yaml
spec:
  securityContext:
    runAsNonRoot: true
    runAsUser: 1000
    fsGroup: 1000
    seccompProfile:
      type: RuntimeDefault
  containers:
  - name: bankmanagementsystem
    securityContext:
      allowPrivilegeEscalation: false
      readOnlyRootFilesystem: true
      capabilities:
        drop:
        - ALL
```

---

## Technology-Specific Notes

### .NET 6 Windows Forms Application

**Important Considerations:**

1. **Windows Forms in Containers:**
   - This is a Windows Forms desktop application, which typically requires a GUI
   - The containerized version runs in headless mode with health check endpoint
   - Not suitable for interactive GUI operations in container environment
   - Consider migrating to ASP.NET Core Web API for full cloud-native support

2. **Base Image:**
   - Using explicit base image: `Regis/Repo/Test`
   - Ensure this base image is compatible with .NET 6 runtime
   - For production, consider official Microsoft images:
     - `mcr.microsoft.com/dotnet/aspnet:6.0` for ASP.NET Core
     - `mcr.microsoft.com/dotnet/runtime:6.0` for console apps

3. **Health Check Implementation:**
   - Custom HTTP listener on port 8080
   - Provides `/health` and `/healthz` endpoints
   - Returns JSON status for Kubernetes probes

4. **Dependencies:**
   - Newtonsoft.Json: JSON serialization
   - log4net: Logging framework
   - HtmlAgilityPack: HTML parsing
   - BankDatabaseAccess: Database layer (project reference)

5. **Configuration:**
   - No appsettings.json (Windows Forms app)
   - Configuration via environment variables
   - Database connection string required

### Migration Recommendations

For production cloud deployment, consider:

1. **Migrate to ASP.NET Core Web API:**
   - Better suited for containerization
   - Native health check support
   - Built-in dependency injection
   - Better performance in containers

2. **Separate UI and API:**
   - Move business logic to Web API
   - Create separate web UI (Blazor, React, Angular)
   - Enables better scalability and deployment

3. **Use Azure Services:**
   - Azure SQL Database for data persistence
   - Azure Key Vault for secrets management
   - Azure Application Insights for monitoring
   - Azure AD (Entra ID) for authentication

### Performance Tuning

**Resource Limits:**
```yaml
resources:
  requests:
    cpu: "250m"      # Minimum CPU
    memory: "512Mi"  # Minimum memory
  limits:
    cpu: "500m"      # Maximum CPU
    memory: "1Gi"    # Maximum memory
```

**Horizontal Pod Autoscaling:**
```bash
kubectl autoscale deployment bankmanagementsystem \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n bankmanagementsystem
```

**Readiness/Liveness Probe Tuning:**
- `initialDelaySeconds`: Increase if app takes longer to start
- `periodSeconds`: How often to check
- `timeoutSeconds`: How long to wait for response
- `failureThreshold`: How many failures before restart

---

## Monitoring and Observability

### Application Insights Integration

1. **Add Application Insights SDK:**
   ```xml
   <PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />
   ```

2. **Configure in Program.cs:**
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```

3. **Set instrumentation key:**
   ```bash
   kubectl create secret generic appinsights-secret \
     --from-literal=APPINSIGHTS_INSTRUMENTATIONKEY="<your-key>" \
     -n bankmanagementsystem
   ```

### Prometheus Metrics

1. **Install Prometheus:**
   ```bash
   helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
   helm install prometheus prometheus-community/kube-prometheus-stack
   ```

2. **Add ServiceMonitor:**
   ```yaml
   apiVersion: monitoring.coreos.com/v1
   kind: ServiceMonitor
   metadata:
     name: bankmanagementsystem-metrics
     namespace: bankmanagementsystem
   spec:
     selector:
       matchLabels:
         app: bankmanagementsystem
     endpoints:
     - port: http
       path: /metrics
   ```

### Logging

**View logs:**
```bash
# Real-time logs
kubectl logs -f deployment/bankmanagementsystem -n bankmanagementsystem

# Logs from all pods
kubectl logs -l app=bankmanagementsystem -n bankmanagementsystem --tail=100

# Export logs
kubectl logs deployment/bankmanagementsystem -n bankmanagementsystem > app.log
```

**Azure Log Analytics:**
```bash
# Enable container insights
az aks enable-addons \
  --resource-group $RESOURCE_GROUP \
  --name $CLUSTER_NAME \
  --addons monitoring
```

---

## Scaling and High Availability

### Horizontal Pod Autoscaling

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: bankmanagementsystem-hpa
  namespace: bankmanagementsystem
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: bankmanagementsystem
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Pod Disruption Budget

```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: bankmanagementsystem-pdb
  namespace: bankmanagementsystem
spec:
  minAvailable: 1
  selector:
    matchLabels:
      app: bankmanagementsystem
```

---

## Backup and Disaster Recovery

### Database Backup

```bash
# Automated backup using Azure SQL
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server <sql-server-name> \
  --name BankDB \
  --backup-storage-redundancy Zone
```

### Application State

- Store application state in external services (database, cache)
- Avoid storing state in containers (ephemeral)
- Use persistent volumes for file storage if needed

---

## CI/CD Integration

### Azure DevOps Pipeline Example

```yaml
trigger:
  branches:
    include:
    - main
  paths:
    include:
    - Backend/BankManagementSystem/*

pool:
  vmImage: 'ubuntu-latest'

variables:
  acrName: '<your-acr-name>'
  imageName: 'bankmanagementsystem'
  imageTag: '$(Build.BuildId)'

stages:
- stage: Build
  jobs:
  - job: BuildAndPush
    steps:
    - task: Docker@2
      inputs:
        containerRegistry: 'ACR-Connection'
        repository: '$(imageName)'
        command: 'buildAndPush'
        Dockerfile: 'Backend/BankManagementSystem/Dockerfile'
        buildContext: 'Backend'
        tags: |
          $(imageTag)
          latest

- stage: Deploy
  dependsOn: Build
  jobs:
  - job: DeployToAKS
    steps:
    - task: Kubernetes@1
      inputs:
        connectionType: 'Azure Resource Manager'
        azureSubscriptionEndpoint: 'Azure-Connection'
        azureResourceGroup: '$(resourceGroup)'
        kubernetesCluster: '$(aksCluster)'
        command: 'apply'
        arguments: '-f Backend/BankManagementSystem/kubernetes/'
```

---

## Support and Resources

### Documentation
- [.NET 6 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6)
- [Azure AKS Documentation](https://docs.microsoft.com/en-us/azure/aks/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Docker Documentation](https://docs.docker.com/)

### Useful Commands Reference

```bash
# AKS Management
az aks list --output table
az aks show --resource-group $RG --name $CLUSTER
az aks scale --resource-group $RG --name $CLUSTER --node-count 3

# Kubectl Shortcuts
alias k=kubectl
alias kgp='kubectl get pods'
alias kgs='kubectl get svc'
alias kgd='kubectl get deployments'
alias kl='kubectl logs -f'

# Quick Health Check
kubectl get pods -n bankmanagementsystem -o wide
kubectl port-forward -n bankmanagementsystem svc/bankmanagementsystem-service 8080:80 &
curl http://localhost:8080/health
```

---

## Conclusion

This deployment guide provides comprehensive instructions for deploying the BankManagementSystem .NET 6 Windows Forms application to Azure AKS. Follow the steps carefully, and refer to the troubleshooting section for common issues.

For production deployments, ensure you:
- ✅ Use proper secrets management (Azure Key Vault)
- ✅ Enable monitoring and logging (Application Insights)
- ✅ Configure autoscaling and high availability
- ✅ Implement network policies and RBAC
- ✅ Regular security scanning and updates
- ✅ Backup and disaster recovery procedures

**Next Steps:**
1. Review and customize Kubernetes manifests for your environment
2. Set up CI/CD pipeline for automated deployments
3. Configure monitoring and alerting
4. Implement security best practices
5. Plan for scaling and performance optimization

For questions or issues, refer to the troubleshooting section or consult the official documentation links provided.

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**Maintained By:** DevOps Team
