#!/bin/bash

echo "Add Label to Container app"
labelgreen="Green"

previousRevisionName=$(az containerapp revision list -n $imageName -g $resourceGroupName --query '[0].name' | tr -d '"')     
latestRevisionName="$imageName--$tag"
echo $previousRevisionName
#Update Container app with updated API
echo "Updating the Container APP $imageName"
az containerapp update --name $imageName --resource-group $resourceGroupName --image $containerRegistryName.azurecr.io/$imageName:$tag --revision-suffix $tag --set-env-vars REVISION_COMMIT_ID=$tag