#!/bin/bash

export resourceGroupName="APIM-DEVOPS"
export location="westeurope"
export APP_NAME="solar"
export APP_ENV="dev"

echo "Starting Infrastructure provisioning..."


echo "Creating resource group..."
az group create --name $resourceGroupName --location $location

echo "Deploying main Bicep file..."
#az deployment group create --confirm-with-what-if --resource-group $resourceGroupName --template-file main.bicep  --parameters main.bicepparam

az deployment group create --resource-group $resourceGroupName --template-file main.bicep  --parameters main.bicepparam