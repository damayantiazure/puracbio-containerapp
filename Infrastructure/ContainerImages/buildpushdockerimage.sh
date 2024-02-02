registry=$registry
registryname=$registryname
imageName=$imageName
tag=$tag
APP_NAME=$APP_NAME
APP_ENV=$APP_ENV

echo $registry
echo $imageName
echo $tag

echo "Login to Azure Container Registry"
accessToken=$(az acr login --name $registryname --expose-token --output tsv --query accessToken)
docker login $registry --username 00000000-0000-0000-0000-000000000000 --password $accessToken

echo "Building Images with Tag '${imageName}:${tag}'"
docker build -t ${registry}/${imageName}:${tag} .

echo "Pushing to '$registry'"
docker push ${registry}/${imageName}:${tag}
