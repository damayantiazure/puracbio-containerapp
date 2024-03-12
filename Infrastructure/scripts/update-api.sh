#!/bin/bash


# export resourceGroupName="APIM-DEVOPS"
# export location="westeurope"
# export APP_NAME="solar"
# export APP_ENV="dev"

echo "Updating API products..."

echo "Deploying products Bicep file..."
az deployment group create --resource-group $resourceGroupName --template-file 'Infrastructure/API-Products/products.bicep'  --parameters 'Infrastructure/API-Products/products.bicepparam'