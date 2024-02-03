echo $registry
echo "Login to Azure Container Registry"
accessToken=$(az acr login --name solar201contregistrydev.azurecr.io --expose-token --output tsv --query accessToken)
docker login solar201contregistrydev.azurecr.io --username 00000000-0000-0000-0000-000000000000 --password $accessToken

echo "Building Images with Tag '${imageName}:${tag}'"
docker build -t ${registry}/${imageName}:${tag} .

echo "Pushing to '$registry'"
docker push ${registry}/${imageName}:${tag}