#!/bin/bash

latestRevisionName=$(az containerapp revision list -n $imageName -g $resourceGroupName --query '[0].name' | tr -d '"')  

echo "Health check for the newly deployed app"

APP_DOMAIN=$(az containerapp env show -g $resourceGroupName -n $acaEnvName --query properties.defaultDomain -o tsv | tr -d '\r\n')
      
echo "Invoking https://$latestRevisionName.$APP_DOMAIN/health"
status_code=$(curl --write-out %{http_code} --silent --output /dev/null "https://$latestRevisionName.$APP_DOMAIN/health")
echo "status_code: $status_code"

if [[ "$status_code" -ne 200 ]] ; then
    echo "Site status changed to - failure to establish a connection to the app"           

    echo "Deactivating the revision $latestRevisionName "
    az containerapp revision deactivate -g $resourceGroupName --revision $latestRevisionName

    echo "Restoring traffic 100% to older revision - $previousRevisionName"  
else
    echo "Restoring traffic 100% to the new revision - $latestRevisionName"
    az containerapp ingress traffic set -n $imageName -g $resourceGroupName --revision-weight $latestRevisionName=100
    exit 0
fi
