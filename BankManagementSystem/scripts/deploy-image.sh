#!/bin/bash
set -e
set -o pipefail

# Deploy to Azure AKS Script for BankManagementSystem
# This script deploys the application to Azure Kubernetes Service

echo "=========================================="
echo "BankManagementSystem - Deploy to Azure AKS"
echo "=========================================="
echo ""

# Prompt for Azure configuration
echo "Azure AKS Configuration"
echo "----------------------"
read -p "Enter Azure Resource Group name: " RESOURCE_GROUP
read -p "Enter AKS Cluster name: " CLUSTER_NAME
read -p "Enter Docker image URI (e.g., myregistry.azurecr.io/bankmanagementsystem:latest): " IMAGE_URI

if [ -z "$RESOURCE_GROUP" ] || [ -z "$CLUSTER_NAME" ] || [ -z "$IMAGE_URI" ]; then
    echo "ERROR: All fields are required."
    exit 1
fi

# Prompt for environment variables
echo ""
echo "Application Configuration (Optional)"
echo "-----------------------------------"
read -p "Enter DATABASE_CONNECTION_STRING (or press Enter to skip): " DATABASE_CONNECTION_STRING

# Configure kubectl
echo ""
echo "Configuring kubectl for AKS cluster..."
echo "--------------------------------------"
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$CLUSTER_NAME" --overwrite-existing

if [ $? -ne 0 ]; then
    echo "ERROR: Failed to configure kubectl. Please check your Azure credentials and cluster details."
    exit 1
fi

# Verify cluster connectivity
echo ""
echo "Verifying cluster connectivity..."
kubectl cluster-info

if [ $? -ne 0 ]; then
    echo "ERROR: Cannot connect to Kubernetes cluster."
    exit 1
fi

# Update manifests with actual values
echo ""
echo "Updating Kubernetes manifests..."
echo "--------------------------------"

# Create temporary directory for updated manifests
TEMP_DIR=$(mktemp -d)
cp -r kubernetes/* "$TEMP_DIR/"

# Replace placeholders
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" "$TEMP_DIR/deployment.yaml"

if [ -n "$DATABASE_CONNECTION_STRING" ]; then
    sed -i "s|{{DATABASE_CONNECTION_STRING}}|$DATABASE_CONNECTION_STRING|g" "$TEMP_DIR/deployment.yaml"
else
    # Remove the DATABASE_CONNECTION_STRING env var if not provided
    sed -i '/{{DATABASE_CONNECTION_STRING}}/d' "$TEMP_DIR/deployment.yaml"
fi

# Apply Kubernetes manifests
echo ""
echo "Deploying to Azure AKS..."
echo "-------------------------"

echo "Creating namespace..."
kubectl apply -f "$TEMP_DIR/namespace.yaml"

echo "Deploying application..."
kubectl apply -f "$TEMP_DIR/deployment.yaml"

echo "Creating service..."
kubectl apply -f "$TEMP_DIR/service.yaml"

echo "Creating ingress..."
kubectl apply -f "$TEMP_DIR/ingress.yaml"

# Wait for deployment rollout
echo ""
echo "Waiting for deployment to complete..."
echo "-------------------------------------"
kubectl rollout status deployment/bankmanagementsystem -n bankmanagementsystem --timeout=5m

if [ $? -ne 0 ]; then
    echo "WARNING: Deployment rollout did not complete successfully."
    echo "Check the status with: kubectl get pods -n bankmanagementsystem"
fi

# Verify deployment
echo ""
echo "Verifying deployment..."
echo "-----------------------"
kubectl get pods,svc,ingress -n bankmanagementsystem

# Get ingress URL
echo ""
echo "=========================================="
echo "DEPLOYMENT COMPLETE!"
echo "=========================================="
echo ""
echo "Application deployed to namespace: bankmanagementsystem"
echo ""
echo "To check application status:"
echo "  kubectl get pods -n bankmanagementsystem"
echo ""
echo "To view logs:"
echo "  kubectl logs -f deployment/bankmanagementsystem -n bankmanagementsystem"
echo ""
echo "To access the application:"
echo "  kubectl port-forward -n bankmanagementsystem svc/bankmanagementsystem-service 8080:80"
echo "  Then open: http://localhost:8080/health"
echo ""
echo "Ingress host: bankmanagementsystem.example.com"
echo "Note: Update your DNS or /etc/hosts to point to the ingress IP"
echo "=========================================="

# Cleanup
rm -rf "$TEMP_DIR"
