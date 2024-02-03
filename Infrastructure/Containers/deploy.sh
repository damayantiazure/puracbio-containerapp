#!/bin/bash


export resourceGroupName=$resourceGroupName
export location=$location
export APP_NAME=$APP_NAME
export APP_ENV=$APP_ENV
export TAG_NAME=$tag
export TAG_NAME=$tag
export imageName=$imageName

echo "Starting deploying the app provisioning..."


echo "Deploying app Bicep file..."
az deployment group create --resource-group 'APIM-DEVOPS' --template-file 'Infrastructure/Containers/app.bicep'  --parameters 'Infrastructure/Containers/app.bicepparam'