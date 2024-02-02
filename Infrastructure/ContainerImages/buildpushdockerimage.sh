registry=$registry
imageName=$imageName
tag=$tag
APP_NAME=$APP_NAME
APP_ENV=$APP_ENV

echo $registry
echo $imageName
echo $tag

URL=$registry
CLEAN_URL=$(echo $URL | tr -d '\r')

echo "Login to Azure Container Registry"
accessToken=$(az acr login --name $URL --expose-token --output tsv --query accessToken)
docker login $registry --username 00000000-0000-0000-0000-000000000000 --password $accessToken
      
echo "Building Images with Tag '${imageName}:${tag}'"
docker build -t $registry/$imageName:$tag -f ./src/WebApis/ComplianceWebApi/Dockerfile .
      
echo "Pushing to '$registry'"
docker push $registry/$imageName:$tag