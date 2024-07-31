#!/bin/bash

echo "Login to Azure Container Registry"
accessToken=$(az acr login --name $registry --expose-token --output tsv --query accessToken)
docker login $registry --username 00000000-0000-0000-0000-000000000000 --password $accessToken

echo "Building Images with Tag '${imageName}:${tag}'"
#docker build -t ${registry}/${imageName}:${tag} -f ./containerapps-albumapi/src/dockerfile .
cd containerapps-albumapi/src
docker build -t ${registry}/${apiimageName}:${tag} -f dockerfile .

cd containerapps-albumui/src
docker build -t ${registry}/${uiimageName}:${tag} -f dockerfile .

echo "Pushing to '$registry'"
docker push ${registry}/${apiimageName}:${tag}
docker push ${registry}/${uiimageName}:${tag}