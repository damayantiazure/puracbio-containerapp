

$resourceGroupName = "APIM-DEVOPS"
$location = "westeurope"

Write-OutPut "Starting deploying the app provisioning..."



Write-OutPut "Deploying app Bicep file..."
az deployment group create --resource-group $resourceGroupName --template-file 'app.bicep'  --parameters app.bicepparam