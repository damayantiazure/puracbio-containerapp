#!/bin/bash

echo "Starting script...$tag and file name $fileName"
az config set extension.use_dynamic_install=yes_without_prompt
az extension add -n containerapp

nextRevisionName="$imageName--$tag"
previousRevisionName=$(az containerapp revision list -n $imageName -g $resourceGroupName --query '[0].name')

prevNameWithoutQuites=$(echo $previousRevisionName | tr -d "\"")        # using sed echo $pname | sed "s/\"//g"
echo 'Previous revision name: ' $prevNameWithoutQuites
echo 'Next revision name: ' $nextRevisionName

sed -i "s/PREV/$prevNameWithoutQuites/g" /Infrastructure/Containers/$fileName 
sed -i "s/NEXT/$nextRevisionName/g" /Infrastructure/Containers/$fileName 


cat /Infrastructure/Containers/$FileName