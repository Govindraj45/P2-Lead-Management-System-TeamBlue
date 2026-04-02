#!/bin/bash
# ============================================================================
# Azure Resource Provisioning Script — Lead Management System
# ============================================================================
# This script creates ALL Azure resources needed for the LMS deployment.
#
# PREREQUISITES:
#   1. Azure CLI installed:  brew install azure-cli
#   2. Logged in:            az login
#   3. Subscription set:     az account set --subscription "<your-subscription-id>"
#
# USAGE:
#   chmod +x azure/provision.sh
#   ./azure/provision.sh
# ============================================================================

set -euo pipefail

# ===================== CONFIGURATION =====================
RESOURCE_GROUP="rg-lead-management"
LOCATION="centralus"

# Container Registry
ACR_NAME="lmscontainerregistry"

# SQL Server
SQL_SERVER_NAME="lms-sql-server-$(openssl rand -hex 4)"
SQL_DB_NAME="CRM_LeadManagement"
SQL_ADMIN_USER="lmsadmin"
SQL_ADMIN_PASSWORD="LmsP@ssw0rd2026!"

# Redis
REDIS_NAME="lms-redis-cache-$(openssl rand -hex 4)"

# App Service
APP_PLAN_NAME="lms-app-plan"
API_APP_NAME="lms-api-app-$(openssl rand -hex 4)"
FRONTEND_APP_NAME="lms-frontend-app-$(openssl rand -hex 4)"

# JWT
JWT_SECRET="LmsJwtSuperSecretKey2026ThatIsLongEnough!"
# =========================================================

echo "============================================"
echo " Lead Management System — Azure Provisioning"
echo "============================================"
echo ""

# ---- 1. Resource Group ----
echo "[1/7] Creating Resource Group: $RESOURCE_GROUP"
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output none
echo "  ✔ Resource Group created"

# ---- 2. Azure Container Registry ----
echo "[2/7] Creating Container Registry: $ACR_NAME"
az acr create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$ACR_NAME" \
  --sku Basic \
  --admin-enabled true \
  --output none
echo "  ✔ ACR created"

ACR_LOGIN_SERVER=$(az acr show --name "$ACR_NAME" --query loginServer -o tsv)
ACR_PASSWORD=$(az acr credential show --name "$ACR_NAME" --query "passwords[0].value" -o tsv)
echo "  → Login Server: $ACR_LOGIN_SERVER"

# ---- 3. Azure SQL Database ----
echo "[3/7] Creating Azure SQL Server: $SQL_SERVER_NAME"
az sql server create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$SQL_SERVER_NAME" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD" \
  --location "$LOCATION" \
  --output none
echo "  ✔ SQL Server created"

echo "  → Creating firewall rule (allow Azure services)..."
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "AllowAzureServices" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none

echo "  → Creating database: $SQL_DB_NAME"
az sql db create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DB_NAME" \
  --service-objective Basic \
  --output none
echo "  ✔ SQL Database created"

SQL_CONNECTION_STRING="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Database=${SQL_DB_NAME};User Id=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};Encrypt=True;TrustServerCertificate=False;"

# ---- 4. Azure Cache for Redis ----
echo "[4/7] Creating Azure Cache for Redis: $REDIS_NAME"
echo "  (This takes 5-15 minutes...)"
az redis create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$REDIS_NAME" \
  --location "$LOCATION" \
  --sku Basic \
  --vm-size C0 \
  --output none
echo "  ✔ Redis created"

REDIS_KEY=$(az redis list-keys --resource-group "$RESOURCE_GROUP" --name "$REDIS_NAME" --query primaryKey -o tsv)
REDIS_CONNECTION_STRING="${REDIS_NAME}.redis.cache.windows.net:6380,password=${REDIS_KEY},ssl=True,abortConnect=False"

# ---- 5. App Service Plan (Linux) ----
echo "[5/7] Creating App Service Plan: $APP_PLAN_NAME"
az appservice plan create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_PLAN_NAME" \
  --is-linux \
  --sku B1 \
  --output none
echo "  ✔ App Service Plan created (Linux B1)"

# ---- 6. Backend App Service ----
echo "[6/7] Creating Backend App Service: $API_APP_NAME"
az webapp create \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_PLAN_NAME" \
  --name "$API_APP_NAME" \
  --deployment-container-image-name "${ACR_LOGIN_SERVER}/lms-api:latest" \
  --output none

az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$API_APP_NAME" \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000 \
    WEBSITES_PORT=5000 \
    "ConnectionStrings__DefaultConnection=${SQL_CONNECTION_STRING}" \
    "Redis__ConnectionString=${REDIS_CONNECTION_STRING}" \
    "Jwt__Secret=${JWT_SECRET}" \
    Jwt__Issuer=LeadManagementSystem \
    Jwt__Audience=LeadManagementSystem \
    Jwt__ExpirationMinutes=60 \
    DOCKER_REGISTRY_SERVER_URL="https://${ACR_LOGIN_SERVER}" \
    DOCKER_REGISTRY_SERVER_USERNAME="${ACR_NAME}" \
    DOCKER_REGISTRY_SERVER_PASSWORD="${ACR_PASSWORD}" \
  --output none
echo "  ✔ Backend App Service created & configured"

# ---- 7. Frontend App Service ----
echo "[7/7] Creating Frontend App Service: $FRONTEND_APP_NAME"
az webapp create \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_PLAN_NAME" \
  --name "$FRONTEND_APP_NAME" \
  --deployment-container-image-name "${ACR_LOGIN_SERVER}/lms-frontend:latest" \
  --output none

az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$FRONTEND_APP_NAME" \
  --settings \
    "API_BACKEND_URL=https://${API_APP_NAME}.azurewebsites.net" \
    WEBSITES_PORT=80 \
    DOCKER_REGISTRY_SERVER_URL="https://${ACR_LOGIN_SERVER}" \
    DOCKER_REGISTRY_SERVER_USERNAME="${ACR_NAME}" \
    DOCKER_REGISTRY_SERVER_PASSWORD="${ACR_PASSWORD}" \
  --output none
echo "  ✔ Frontend App Service created & configured"

# ---- Enable CORS on backend for frontend ----
echo "  → Configuring CORS on backend..."
az webapp cors add \
  --resource-group "$RESOURCE_GROUP" \
  --name "$API_APP_NAME" \
  --allowed-origins "https://${FRONTEND_APP_NAME}.azurewebsites.net" \
  --output none

# ============================================
# SUMMARY
# ============================================
echo ""
echo "============================================"
echo " PROVISIONING COMPLETE"
echo "============================================"
echo ""
echo "Resources created in: $RESOURCE_GROUP"
echo ""
echo "  ACR Login Server:    $ACR_LOGIN_SERVER"
echo "  SQL Server:          ${SQL_SERVER_NAME}.database.windows.net"
echo "  Redis:               ${REDIS_NAME}.redis.cache.windows.net"
echo "  Backend URL:         https://${API_APP_NAME}.azurewebsites.net"
echo "  Frontend URL:        https://${FRONTEND_APP_NAME}.azurewebsites.net"
echo ""
echo "============================================"
echo " AZURE DEVOPS PIPELINE VARIABLES"
echo "============================================"
echo "Set these as SECRET variables in your pipeline:"
echo ""
echo "  sqlConnectionString  = ${SQL_CONNECTION_STRING}"
echo "  redisConnectionString = ${REDIS_CONNECTION_STRING}"
echo "  jwtSecret             = ${JWT_SECRET}"
echo ""
echo "Update these in azure-pipelines.yml:"
echo ""
echo "  acrName:              $ACR_NAME"
echo "  backendAppService:    $API_APP_NAME"
echo "  frontendAppService:   $FRONTEND_APP_NAME"
echo ""
echo "============================================"
echo " NEXT STEPS"
echo "============================================"
echo "1. Go to https://dev.azure.com → your project"
echo "2. Project Settings → Service Connections:"
echo "   a. New → Docker Registry → Azure Container Registry → name: 'ACRServiceConnection'"
echo "   b. New → Azure Resource Manager → Service Principal → name: 'AzureServiceConnection'"
echo "3. Pipelines → New Pipeline → select your repo → Existing YAML → azure-pipelines.yml"
echo "4. Edit pipeline → Variables → add the 3 secrets above"
echo "5. Run the pipeline!"
echo ""
echo "To DESTROY all resources:  az group delete --name $RESOURCE_GROUP --yes --no-wait"
