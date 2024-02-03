#!/bin/bash


export resourceGroupName=$resourceGroupName
export location=$location
export APP_NAME=$APP_NAME
export APP_ENV=$APP_ENV
export containerRegistryName=$containerRegistryName
export tagNameE=$tag
export imageName=$imageName
export uamiName=$uamiName
export appInsightName=$appInsightName

echo "Starting deploying the app provisioning..."


echo "Deploying app Bicep file..."
az deployment group create --resource-group 'APIM-DEVOPS' --template-file 'Infrastructure/Containers/app.bicep'