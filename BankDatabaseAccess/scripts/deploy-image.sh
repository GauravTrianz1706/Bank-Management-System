#!/bin/bash

# Deploy BankDatabaseAccess to Azure AKS
# This script deploys the application to Azure Kubernetes Service (AKS)

set -e
set -o pipefail

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}BankDatabaseAccess - Deploy to Azure AKS${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Prompt for Azure configuration
echo -e "${YELLOW}Azure AKS Configuration${NC}"
read -p "Enter Azure resource group name: " RESOURCE_GROUP
read -p "Enter AKS cluster name: " CLUSTER_NAME

# Validate inputs
if [ -z "$RESOURCE_GROUP" ] || [ -z "$CLUSTER_NAME" ]; then
    echo -e "${RED}Error: Resource group and cluster name are required.${NC}"
    exit 1
fi

# Prompt for Docker image URI
echo ""
read -p "Enter Docker image URI (e.g., myregistry.azurecr.io/bankdatabaseaccess:latest): " IMAGE_URI

if [ -z "$IMAGE_URI" ]; then
    echo -e "${RED}Error: Docker image URI is required.${NC}"
    exit 1
fi

# Prompt for environment-specific configuration
echo ""
echo -e "${YELLOW}Application Configuration${NC}"
read -p "Enter DATABASE_SERVER (or press Enter to skip): " DATABASE_SERVER
read -p "Enter DATABASE_NAME (or press Enter to skip): " DATABASE_NAME
read -p "Enter DATABASE_USER (or press Enter to skip): " DATABASE_USER
read -sp "Enter DATABASE_PASSWORD (or press Enter to skip): " DATABASE_PASSWORD
echo ""

# Set default values if not provided
DATABASE_SERVER=${DATABASE_SERVER:-sqlserver}
DATABASE_NAME=${DATABASE_NAME:-BankDB}
DATABASE_USER=${DATABASE_USER:-sa}

# Configure kubectl to use AKS cluster
echo ""
echo -e "${YELLOW}Configuring kubectl for AKS cluster...${NC}"
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$CLUSTER_NAME" --overwrite-existing

if [ $? -ne 0 ]; then
    echo -e "${RED}Failed to configure kubectl. Please check your Azure credentials and cluster details.${NC}"
    exit 1
fi

# Verify cluster connectivity
echo ""
echo -e "${YELLOW}Verifying cluster connectivity...${NC}"
kubectl cluster-info

if [ $? -ne 0 ]; then
    echo -e "${RED}Failed to connect to cluster. Please check your configuration.${NC}"
    exit 1
fi

# Update Kubernetes manifests with actual values
echo ""
echo -e "${YELLOW}Updating Kubernetes manifests...${NC}"

# Create temporary directory for updated manifests
TEMP_DIR=$(mktemp -d)
cp -r kubernetes/* "$TEMP_DIR/"

# Replace placeholders in deployment.yaml
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" "$TEMP_DIR/deployment.yaml"
sed -i "s|{{DATABASE_SERVER}}|$DATABASE_SERVER|g" "$TEMP_DIR/deployment.yaml"
sed -i "s|{{DATABASE_NAME}}|$DATABASE_NAME|g" "$TEMP_DIR/deployment.yaml"
sed -i "s|{{DATABASE_USER}}|$DATABASE_USER|g" "$TEMP_DIR/deployment.yaml"

# Create secret for database password if provided
if [ -n "$DATABASE_PASSWORD" ]; then
    echo -e "${YELLOW}Creating Kubernetes secret for database password...${NC}"
    kubectl create secret generic bankdatabaseaccess-secrets \
        --from-literal=database-password="$DATABASE_PASSWORD" \
        --namespace=bankdatabaseaccess \
        --dry-run=client -o yaml | kubectl apply -f -
fi

# Apply Kubernetes manifests
echo ""
echo -e "${YELLOW}Applying Kubernetes manifests...${NC}"

# Apply namespace
echo -e "${YELLOW}Creating namespace...${NC}"
kubectl apply -f "$TEMP_DIR/namespace.yaml"

# Apply deployment
echo -e "${YELLOW}Creating deployment...${NC}"
kubectl apply -f "$TEMP_DIR/deployment.yaml"

# Apply service
echo -e "${YELLOW}Creating service...${NC}"
kubectl apply -f "$TEMP_DIR/service.yaml"

# Apply ingress
echo -e "${YELLOW}Creating ingress...${NC}"
kubectl apply -f "$TEMP_DIR/ingress.yaml"

# Clean up temporary directory
rm -rf "$TEMP_DIR"

# Wait for deployment to be ready
echo ""
echo -e "${YELLOW}Waiting for deployment to be ready...${NC}"
kubectl rollout status deployment/bankdatabaseaccess -n bankdatabaseaccess --timeout=5m

if [ $? -ne 0 ]; then
    echo -e "${RED}Deployment failed to become ready. Checking pod status...${NC}"
    kubectl get pods -n bankdatabaseaccess
    kubectl describe pods -n bankdatabaseaccess
    exit 1
fi

# Verify deployment
echo ""
echo -e "${YELLOW}Verifying deployment...${NC}"
kubectl get pods,svc,ingress -n bankdatabaseaccess

# Get ingress URL
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Completed Successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

INGRESS_IP=$(kubectl get ingress bankdatabaseaccess-ingress -n bankdatabaseaccess -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")

if [ "$INGRESS_IP" != "pending" ] && [ -n "$INGRESS_IP" ]; then
    echo -e "${YELLOW}Application URL:${NC} http://$INGRESS_IP"
else
    echo -e "${YELLOW}Ingress IP is pending. Run the following command to check status:${NC}"
    echo "kubectl get ingress bankdatabaseaccess-ingress -n bankdatabaseaccess"
fi

echo ""
echo -e "${YELLOW}Useful Commands:${NC}"
echo "  View pods:        kubectl get pods -n bankdatabaseaccess"
echo "  View logs:        kubectl logs -f deployment/bankdatabaseaccess -n bankdatabaseaccess"
echo "  View services:    kubectl get svc -n bankdatabaseaccess"
echo "  View ingress:     kubectl get ingress -n bankdatabaseaccess"
echo "  Scale deployment: kubectl scale deployment/bankdatabaseaccess --replicas=3 -n bankdatabaseaccess"
echo "  Delete deployment: kubectl delete namespace bankdatabaseaccess"
echo ""
