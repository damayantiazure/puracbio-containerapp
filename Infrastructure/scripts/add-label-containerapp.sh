#!/bin/bash

echo "Add Label to Container app"
labelgreen="Green"

previousRevisionName=$(az containerapp revision list -n $(imageName) -g $(resourceGroupName) --query '[0].name' | tr -d '"')     
latestRevisionName="$(imageName)--$(tag)"

echo $previousRevisionName
echo $latestRevisionName

#Update Container app with updated API
echo "Updating the Container APP $(imageName)"
az containerapp update --name $(imageName) --resource-group $(resourceGroupName) --image $(containerRegistryName).azurecr.io/$(imageName):$(tag) --revision-suffix $(tag) --set-env-vars REVISION_COMMIT_ID=$(tag)
      
#give that revision a 'green' label
echo "Add a Label green $(imageName)"
az containerapp revision label add --name $(imageName) --resource-group $(resourceGroupName) --label $labelgreen --revision $latestRevisionName