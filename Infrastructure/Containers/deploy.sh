#!/bin/bash


export resourceGroupName="APIM-DEVOPS"
export location="westeurope"
export APP_NAME="containerapp-demo-api"
export APP_ENV="Containerapp-demo-env"
export TAG_NAME="beta2"

echo "Starting deploying the app provisioning..."


echo "Deploying app Bicep file..."
az deployment group create --resource-group 'APIM-DEVOPS' --template-file 'app.bicep'  