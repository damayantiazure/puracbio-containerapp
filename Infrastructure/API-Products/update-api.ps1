

$resourceGroupName = "APIM-DEVOPS"
$location = "westeurope"

Write-OutPut "Updating API products..."

Write-OutPut "Deploying products Bicep file..."
az deployment group create --resource-group $resourceGroupName --template-file products.bicep  --parameters products.bicepparam