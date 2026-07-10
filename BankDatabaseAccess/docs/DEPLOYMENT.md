# BankDatabaseAccess - Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Structure](#project-structure)
4. [Local Development Setup](#local-development-setup)
5. [Docker Deployment](#docker-deployment)
6. [Azure AKS Deployment](#azure-aks-deployment)
7. [Configuration Management](#configuration-management)
8. [Troubleshooting](#troubleshooting)
9. [Security Considerations](#security-considerations)
10. [Technology-Specific Notes](#technology-specific-notes)

---

## Overview

BankDatabaseAccess is a .NET 6.0 class library that provides database access functionality for the Bank Management System. This guide covers containerization and deployment to Azure Kubernetes Service (AKS).

**Technology Stack:**
- .NET 6.0 (Class Library)
- SQL Server (Database)
- log4net (Logging)
- Newtonsoft.Json (JSON serialization)

**Important Note:** This is a class library project and is typically not deployed independently. It should be referenced by the main application (BankManagementSystem). However, containerization artifacts are provided for reference and testing purposes.

---

## Prerequisites

### Required Software
- **Docker Desktop** (v20.10+)
  - Windows: [Download Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
  - macOS: [Download Docker Desktop for Mac](https://www.docker.com/products/docker-desktop)
  - Linux: [Install Docker Engine](https://docs.docker.com/engine/install/)

- **Azure CLI** (v2.40+)
  ```bash
  # Windows (using winget)
  winget install Microsoft.AzureCLI
  
  # macOS
  brew install azure-cli
  
  # Linux
  curl -sL https://aka.ms/InstallAzureCLI | sudo bash
  ```

- **kubectl** (v1.24+)
  ```bash
  # Windows (using winget)
  winget install Kubernetes.kubectl
  
  # macOS
  brew install kubectl
  
  # Linux
  curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
  sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl
  ```

- **.NET 6.0 SDK** (for local development)
  - [Download .NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

### Azure Requirements
- Active Azure subscription
- Azure Container Registry (ACR) or Docker Hub account
- Azure Kubernetes Service (AKS) cluster
- Appropriate permissions to create and manage resources

### External Services
- **SQL Server Database**: Required for database operations
  - Connection string format: `Server=<server>;Database=<db>;User Id=<user>;Password=<password>;TrustServerCertificate=True;`

---

## Project Structure

```
BankDatabaseAccess/
├── Dockerfile                      # Multi-stage Docker build file
├── .dockerignore                   # Docker ignore patterns
├── docker-compose.yml              # Docker Compose configuration
├── BankDatabaseAccess.csproj       # .NET project file
├── DatabaseConnection.cs           # Database connection management
├── EntityModel/                    # Entity models
│   ├── CustomerModel.cs
│   ├── EmployeeModel.cs
│   └── PersonModel.cs
├── DatabaseOperation/              # Database operations
│   ├── CustomerOperation.cs
│   ├── EmployeeOperations.cs
│   ├── DataReader.cs
│   └── IOperations.cs
├── scripts/
│   ├── build-push.sh              # Linux/macOS build and push script
│   ├── build-push.bat             # Windows build and push script
│   ├── deploy-image.sh            # Linux/macOS AKS deployment script
│   └── deploy-image.bat           # Windows AKS deployment script
├── kubernetes/
│   ├── namespace.yaml             # Kubernetes namespace
│   ├── deployment.yaml            # Kubernetes deployment
│   ├── service.yaml               # Kubernetes service
│   └── ingress.yaml               # Kubernetes ingress
└── docs/
    └── DEPLOYMENT.md              # This file
```

---

## Local Development Setup

### 1. Clone the Repository
```bash
cd /path/to/workspace/Backend/BankDatabaseAccess
```

### 2. Build the Project Locally
```bash
# Restore dependencies
dotnet restore BankDatabaseAccess.csproj

# Build the project
dotnet build BankDatabaseAccess.csproj -c Release

# Run tests (if applicable)
dotnet test
```

### 3. Configure Database Connection
Create a configuration file or set environment variables:

```bash
# Linux/macOS
export DATABASE_SERVER="localhost"
export DATABASE_NAME="BankDB"
export DATABASE_USER="sa"
export DATABASE_PASSWORD="YourStrong@Passw0rd"

# Windows (PowerShell)
$env:DATABASE_SERVER="localhost"
$env:DATABASE_NAME="BankDB"
$env:DATABASE_USER="sa"
$env:DATABASE_PASSWORD="YourStrong@Passw0rd"
```

---

## Docker Deployment

### 1. Build Docker Image Locally

```bash
# Build the image
docker build -f Dockerfile -t bankdatabaseaccess:latest .

# Verify the image
docker images | grep bankdatabaseaccess
```

### 2. Run with Docker Compose

```bash
# Start the container
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the container
docker-compose down
```

### 3. Test the Container

```bash
# Check if container is running
docker ps | grep bankdatabaseaccess

# Execute commands inside the container
docker exec -it bankdatabaseaccess bash

# View container logs
docker logs bankdatabaseaccess
```

---

## Azure AKS Deployment

### Step 1: Prepare Azure Resources

#### 1.1 Login to Azure
```bash
az login
az account set --subscription "<your-subscription-id>"
```

#### 1.2 Create Resource Group (if not exists)
```bash
az group create \
  --name bank-app-rg \
  --location eastus
```

#### 1.3 Create Azure Container Registry (if not exists)
```bash
az acr create \
  --resource-group bank-app-rg \
  --name bankappregistry \
  --sku Basic

# Enable admin access (for development)
az acr update --name bankappregistry --admin-enabled true
```

#### 1.4 Create AKS Cluster (if not exists)
```bash
az aks create \
  --resource-group bank-app-rg \
  --name bank-aks-cluster \
  --node-count 2 \
  --node-vm-size Standard_DS2_v2 \
  --enable-managed-identity \
  --attach-acr bankappregistry \
  --generate-ssh-keys
```

### Step 2: Build and Push Docker Image

#### Option A: Using the Build Script (Recommended)

**Linux/macOS:**
```bash
cd /path/to/workspace/Backend/BankDatabaseAccess
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

**Windows:**
```cmd
cd C:\path\to\workspace\Backend\BankDatabaseAccess
scripts\build-push.bat
```

The script will:
1. Prompt for registry type (ACR or Docker Hub)
2. Prompt for registry credentials
3. Build the Docker image
4. Push to the selected registry

#### Option B: Manual Build and Push

**For Azure Container Registry:**
```bash
# Login to ACR
az acr login --name bankappregistry

# Build and tag the image
docker build -f Dockerfile -t bankappregistry.azurecr.io/bankdatabaseaccess:v1.0.0 .

# Push to ACR
docker push bankappregistry.azurecr.io/bankdatabaseaccess:v1.0.0
```

**For Docker Hub:**
```bash
# Login to Docker Hub
docker login

# Build and tag the image
docker build -f Dockerfile -t yourusername/bankdatabaseaccess:v1.0.0 .

# Push to Docker Hub
docker push yourusername/bankdatabaseaccess:v1.0.0
```

### Step 3: Deploy to AKS

#### Option A: Using the Deployment Script (Recommended)

**Linux/macOS:**
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

**Windows:**
```cmd
scripts\deploy-image.bat
```

The script will:
1. Prompt for Azure resource group and AKS cluster name
2. Prompt for Docker image URI
3. Prompt for environment-specific configuration
4. Configure kubectl
5. Apply Kubernetes manifests
6. Wait for deployment to be ready
7. Display deployment status and access information

#### Option B: Manual Deployment

```bash
# Configure kubectl
az aks get-credentials \
  --resource-group bank-app-rg \
  --name bank-aks-cluster \
  --overwrite-existing

# Verify cluster connectivity
kubectl cluster-info

# Update deployment.yaml with your image URI
# Replace {{IMAGE_URI}} with your actual image URI
sed -i 's|{{IMAGE_URI}}|bankappregistry.azurecr.io/bankdatabaseaccess:v1.0.0|g' kubernetes/deployment.yaml

# Update environment variables
sed -i 's|{{DATABASE_SERVER}}|your-sql-server.database.windows.net|g' kubernetes/deployment.yaml
sed -i 's|{{DATABASE_NAME}}|BankDB|g' kubernetes/deployment.yaml
sed -i 's|{{DATABASE_USER}}|sqladmin|g' kubernetes/deployment.yaml

# Create database password secret
kubectl create secret generic bankdatabaseaccess-secrets \
  --from-literal=database-password='YourStrong@Passw0rd' \
  --namespace=bankdatabaseaccess

# Apply Kubernetes manifests
kubectl apply -f kubernetes/namespace.yaml
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# Wait for deployment
kubectl rollout status deployment/bankdatabaseaccess -n bankdatabaseaccess

# Verify deployment
kubectl get pods,svc,ingress -n bankdatabaseaccess
```

### Step 4: Verify Deployment

```bash
# Check pod status
kubectl get pods -n bankdatabaseaccess

# View pod logs
kubectl logs -f deployment/bankdatabaseaccess -n bankdatabaseaccess

# Check service
kubectl get svc -n bankdatabaseaccess

# Check ingress
kubectl get ingress -n bankdatabaseaccess

# Describe pod for detailed information
kubectl describe pod <pod-name> -n bankdatabaseaccess
```

### Step 5: Access the Application

```bash
# Get ingress IP address
kubectl get ingress bankdatabaseaccess-ingress -n bankdatabaseaccess

# Once the IP is assigned, access the application
# http://<INGRESS_IP>
```

---

## Configuration Management

### Environment Variables

The application uses the following environment variables:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `DOTNET_ENVIRONMENT` | .NET environment | Production | No |
| `DATABASE_SERVER` | SQL Server hostname | sqlserver | Yes |
| `DATABASE_NAME` | Database name | BankDB | Yes |
| `DATABASE_USER` | Database username | sa | Yes |
| `DATABASE_PASSWORD` | Database password | - | Yes |
| `CONNECTION_STRING` | Full connection string | - | Yes |
| `LOG_LEVEL` | Logging level | Information | No |
| `LOG4NET_CONFIG` | log4net config path | /app/config/log4net.config | No |
| `TZ` | Timezone | UTC | No |

### Kubernetes Secrets

Store sensitive information in Kubernetes secrets:

```bash
# Create secret for database password
kubectl create secret generic bankdatabaseaccess-secrets \
  --from-literal=database-password='YourStrong@Passw0rd' \
  --namespace=bankdatabaseaccess

# View secrets
kubectl get secrets -n bankdatabaseaccess

# Describe secret
kubectl describe secret bankdatabaseaccess-secrets -n bankdatabaseaccess
```

### ConfigMaps

Store non-sensitive configuration in ConfigMaps:

```bash
# Create ConfigMap from file
kubectl create configmap bankdatabaseaccess-config \
  --from-file=log4net.config=./config/log4net.config \
  --namespace=bankdatabaseaccess

# View ConfigMaps
kubectl get configmaps -n bankdatabaseaccess
```

---

## Troubleshooting

### Common Issues

#### 1. Pod Not Starting

**Symptoms:**
- Pod status: `CrashLoopBackOff`, `Error`, or `ImagePullBackOff`

**Solutions:**
```bash
# Check pod status
kubectl get pods -n bankdatabaseaccess

# View pod logs
kubectl logs <pod-name> -n bankdatabaseaccess

# Describe pod for events
kubectl describe pod <pod-name> -n bankdatabaseaccess

# Check if image exists
docker pull <your-image-uri>
```

**Common Causes:**
- Incorrect image URI
- Missing or incorrect environment variables
- Database connection issues
- Insufficient resources

#### 2. Database Connection Failures

**Symptoms:**
- Application logs show connection errors
- Pods restart frequently

**Solutions:**
```bash
# Verify database credentials
kubectl get secret bankdatabaseaccess-secrets -n bankdatabaseaccess -o yaml

# Test database connectivity from pod
kubectl exec -it <pod-name> -n bankdatabaseaccess -- bash
# Inside pod:
# apt-get update && apt-get install -y telnet
# telnet <database-server> 1433

# Check connection string format
kubectl describe deployment bankdatabaseaccess -n bankdatabaseaccess
```

#### 3. Image Pull Errors

**Symptoms:**
- Pod status: `ImagePullBackOff` or `ErrImagePull`

**Solutions:**
```bash
# For ACR: Verify AKS has access to ACR
az aks check-acr \
  --resource-group bank-app-rg \
  --name bank-aks-cluster \
  --acr bankappregistry.azurecr.io

# Attach ACR to AKS if not attached
az aks update \
  --resource-group bank-app-rg \
  --name bank-aks-cluster \
  --attach-acr bankappregistry

# For Docker Hub: Create image pull secret
kubectl create secret docker-registry dockerhub-secret \
  --docker-server=https://index.docker.io/v1/ \
  --docker-username=<username> \
  --docker-password=<password> \
  --docker-email=<email> \
  --namespace=bankdatabaseaccess

# Update deployment to use the secret
# Add to deployment.yaml under spec.template.spec:
# imagePullSecrets:
# - name: dockerhub-secret
```

#### 4. Ingress Not Working

**Symptoms:**
- Cannot access application via ingress URL
- Ingress IP not assigned

**Solutions:**
```bash
# Check ingress status
kubectl get ingress -n bankdatabaseaccess

# Describe ingress for events
kubectl describe ingress bankdatabaseaccess-ingress -n bankdatabaseaccess

# Verify Application Gateway Ingress Controller is installed
kubectl get pods -n kube-system | grep ingress

# Check service endpoints
kubectl get endpoints -n bankdatabaseaccess

# Test service directly
kubectl port-forward svc/bankdatabaseaccess-service 8080:80 -n bankdatabaseaccess
# Access: http://localhost:8080
```

### Debugging Commands

```bash
# View all resources in namespace
kubectl get all -n bankdatabaseaccess

# View events
kubectl get events -n bankdatabaseaccess --sort-by='.lastTimestamp'

# View pod logs (previous container)
kubectl logs <pod-name> -n bankdatabaseaccess --previous

# Execute commands in pod
kubectl exec -it <pod-name> -n bankdatabaseaccess -- bash

# Port forward for local testing
kubectl port-forward deployment/bankdatabaseaccess 8080:8080 -n bankdatabaseaccess

# View resource usage
kubectl top pods -n bankdatabaseaccess
kubectl top nodes
```

---

## Security Considerations

### 1. Container Security

- **Run as non-root user**: The Dockerfile creates a non-root user for running the application
- **Minimal base image**: Uses official Microsoft .NET runtime images
- **No unnecessary packages**: Avoid installing additional tools in production images
- **Scan images**: Regularly scan images for vulnerabilities

```bash
# Scan image with Azure Container Registry
az acr task run \
  --registry bankappregistry \
  --cmd "mcr.microsoft.com/azure-cli az acr check-health --name bankappregistry" \
  /dev/null
```

### 2. Kubernetes Security

- **Network Policies**: Restrict pod-to-pod communication
- **Pod Security Standards**: Enforce security policies
- **RBAC**: Use Role-Based Access Control
- **Secrets Management**: Use Azure Key Vault for sensitive data

```bash
# Create network policy
kubectl apply -f - <<EOF
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: bankdatabaseaccess-netpol
  namespace: bankdatabaseaccess
spec:
  podSelector:
    matchLabels:
      app: bankdatabaseaccess
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: bankdatabaseaccess
    ports:
    - protocol: TCP
      port: 8080
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 1433  # SQL Server
EOF
```

### 3. Database Security

- **Use Azure SQL Database**: Managed service with built-in security
- **Enable SSL/TLS**: Encrypt database connections
- **Use Managed Identity**: Avoid storing credentials
- **Firewall rules**: Restrict database access

```bash
# Configure Azure SQL firewall to allow AKS
AKS_OUTBOUND_IP=$(az aks show \
  --resource-group bank-app-rg \
  --name bank-aks-cluster \
  --query "networkProfile.loadBalancerProfile.effectiveOutboundIps[0].id" \
  --output tsv)

az sql server firewall-rule create \
  --resource-group bank-app-rg \
  --server <sql-server-name> \
  --name AllowAKS \
  --start-ip-address <aks-outbound-ip> \
  --end-ip-address <aks-outbound-ip>
```

### 4. Secrets Management with Azure Key Vault

```bash
# Create Azure Key Vault
az keyvault create \
  --name bank-app-keyvault \
  --resource-group bank-app-rg \
  --location eastus

# Store database password
az keyvault secret set \
  --vault-name bank-app-keyvault \
  --name database-password \
  --value 'YourStrong@Passw0rd'

# Enable AKS to access Key Vault
az aks enable-addons \
  --resource-group bank-app-rg \
  --name bank-aks-cluster \
  --addons azure-keyvault-secrets-provider

# Create SecretProviderClass
kubectl apply -f - <<EOF
apiVersion: secrets-store.csi.x-k8s.io/v1
kind: SecretProviderClass
metadata:
  name: azure-keyvault
  namespace: bankdatabaseaccess
spec:
  provider: azure
  parameters:
    usePodIdentity: "false"
    useVMManagedIdentity: "true"
    userAssignedIdentityID: "<identity-client-id>"
    keyvaultName: "bank-app-keyvault"
    objects: |
      array:
        - |
          objectName: database-password
          objectType: secret
          objectVersion: ""
    tenantId: "<tenant-id>"
EOF
```

---

## Technology-Specific Notes

### .NET 6.0 Class Library

#### Characteristics
- **Output Type**: Library (DLL)
- **Target Framework**: .NET 6.0
- **Dependencies**: 
  - Newtonsoft.Json (13.0.3)
  - log4net (2.0.15)
  - System.Data.SqlClient (4.8.5)
  - System.Configuration.ConfigurationManager (6.0.0)

#### Deployment Considerations

1. **Not a Standalone Application**: This is a class library and should be referenced by the main application (BankManagementSystem).

2. **Containerization**: While containerization artifacts are provided, this library is typically deployed as part of the main application container.

3. **Database Access**: The library provides database access functionality and requires:
   - SQL Server connection
   - Proper connection string configuration
   - Database credentials

4. **Logging**: Uses log4net for logging. Ensure log4net.config is properly configured.

#### Performance Optimization

```xml
<!-- Add to .csproj for optimized builds -->
<PropertyGroup>
  <PublishTrimmed>true</PublishTrimmed>
  <PublishReadyToRun>true</PublishReadyToRun>
  <PublishSingleFile>false</PublishSingleFile>
</PropertyGroup>
```

#### Memory Management

```bash
# Set memory limits in Kubernetes
# Already configured in deployment.yaml:
# resources:
#   requests:
#     memory: "512Mi"
#   limits:
#     memory: "1Gi"
```

### SQL Server Integration

#### Connection String Format
```
Server=<server>;Database=<db>;User Id=<user>;Password=<password>;TrustServerCertificate=True;
```

#### Azure SQL Database
```
Server=tcp:<server>.database.windows.net,1433;Database=<db>;User Id=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

#### Connection Pooling
```csharp
// Connection pooling is enabled by default in System.Data.SqlClient
// Configure in connection string:
// ;Min Pool Size=5;Max Pool Size=100;Pooling=true;
```

---

## Scaling and High Availability

### Horizontal Pod Autoscaling

```bash
# Create HPA based on CPU usage
kubectl autoscale deployment bankdatabaseaccess \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n bankdatabaseaccess

# View HPA status
kubectl get hpa -n bankdatabaseaccess

# Describe HPA
kubectl describe hpa bankdatabaseaccess -n bankdatabaseaccess
```

### Manual Scaling

```bash
# Scale to 5 replicas
kubectl scale deployment/bankdatabaseaccess --replicas=5 -n bankdatabaseaccess

# Verify scaling
kubectl get pods -n bankdatabaseaccess
```

### Rolling Updates

```bash
# Update image
kubectl set image deployment/bankdatabaseaccess \
  bankdatabaseaccess=bankappregistry.azurecr.io/bankdatabaseaccess:v1.1.0 \
  -n bankdatabaseaccess

# Monitor rollout
kubectl rollout status deployment/bankdatabaseaccess -n bankdatabaseaccess

# View rollout history
kubectl rollout history deployment/bankdatabaseaccess -n bankdatabaseaccess

# Rollback to previous version
kubectl rollout undo deployment/bankdatabaseaccess -n bankdatabaseaccess
```

---

## Monitoring and Observability

### Application Insights Integration

```bash
# Install Application Insights agent
kubectl apply -f https://github.com/microsoft/Application-Insights-K8s-Codeless-Attach/releases/download/v1.0.0/application-insights-k8s-codeless-attach.yaml

# Configure instrumentation key
kubectl create secret generic appinsights-secret \
  --from-literal=instrumentation-key='<your-instrumentation-key>' \
  --namespace=bankdatabaseaccess
```

### Prometheus Monitoring

```bash
# Add Prometheus annotations to deployment
# Already included in deployment.yaml:
# annotations:
#   prometheus.io/scrape: "true"
#   prometheus.io/port: "8080"
#   prometheus.io/path: "/metrics"
```

### Log Aggregation

```bash
# View logs from all pods
kubectl logs -l app=bankdatabaseaccess -n bankdatabaseaccess --tail=100

# Stream logs
kubectl logs -f deployment/bankdatabaseaccess -n bankdatabaseaccess

# Export logs to Azure Log Analytics
# Configure in AKS cluster settings
```

---

## Cleanup

### Remove Deployment

```bash
# Delete namespace (removes all resources)
kubectl delete namespace bankdatabaseaccess

# Or delete individual resources
kubectl delete -f kubernetes/ingress.yaml
kubectl delete -f kubernetes/service.yaml
kubectl delete -f kubernetes/deployment.yaml
kubectl delete -f kubernetes/namespace.yaml
```

### Remove Docker Images

```bash
# Remove local images
docker rmi bankdatabaseaccess:latest

# Remove from ACR
az acr repository delete \
  --name bankappregistry \
  --repository bankdatabaseaccess \
  --yes
```

### Remove Azure Resources

```bash
# Delete AKS cluster
az aks delete \
  --resource-group bank-app-rg \
  --name bank-aks-cluster \
  --yes --no-wait

# Delete ACR
az acr delete \
  --resource-group bank-app-rg \
  --name bankappregistry \
  --yes

# Delete resource group
az group delete \
  --name bank-app-rg \
  --yes --no-wait
```

---

## Additional Resources

### Documentation
- [.NET 6.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6)
- [Azure Kubernetes Service Documentation](https://docs.microsoft.com/en-us/azure/aks/)
- [Docker Documentation](https://docs.docker.com/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)

### Best Practices
- [.NET Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [AKS Best Practices](https://docs.microsoft.com/en-us/azure/aks/best-practices)
- [Container Security Best Practices](https://docs.microsoft.com/en-us/azure/container-instances/container-instances-image-security)

### Support
- GitHub Issues: [Project Repository]
- Azure Support: [Azure Portal](https://portal.azure.com)
- Community Forums: [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-aks)

---

## Appendix

### A. Complete Environment Variables Reference

```bash
# .NET Configuration
DOTNET_ENVIRONMENT=Production
DOTNET_RUNNING_IN_CONTAINER=true

# Database Configuration
DATABASE_SERVER=your-sql-server.database.windows.net
DATABASE_NAME=BankDB
DATABASE_USER=sqladmin
DATABASE_PASSWORD=YourStrong@Passw0rd
CONNECTION_STRING=Server=your-sql-server.database.windows.net;Database=BankDB;User Id=sqladmin;Password=YourStrong@Passw0rd;TrustServerCertificate=True;

# Logging Configuration
LOG_LEVEL=Information
LOG4NET_CONFIG=/app/config/log4net.config

# Application Settings
TZ=UTC
```

### B. Kubernetes Resource Limits Guide

| Resource Type | Request | Limit | Notes |
|---------------|---------|-------|-------|
| CPU | 250m | 500m | 0.25 to 0.5 cores |
| Memory | 512Mi | 1Gi | 512MB to 1GB |

### C. Port Reference

| Port | Protocol | Purpose |
|------|----------|---------|
| 8080 | TCP | Application port (if applicable) |
| 1433 | TCP | SQL Server connection |

---

**Document Version**: 1.0.0  
**Last Updated**: 2024-01-15  
**Maintained By**: DevOps Team
