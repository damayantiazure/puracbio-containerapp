

$resourceGroupName = "APIM-DEVOPS"
$location = "westeurope"

Write-OutPut "Starting Infrastructure provisioning..."


Write-OutPut "Creating resource group..."
az group create --name $resourceGroupName --location $location

Write-OutPut "Deploying main Bicep file..."
#az deployment group create --confirm-with-what-if --resource-group $resourceGroupName --template-file main.bicep  --parameters main.bicepparam

az deployment group create --resource-group $resourceGroupName --template-file main.bicep  --parameters main.bicepparam