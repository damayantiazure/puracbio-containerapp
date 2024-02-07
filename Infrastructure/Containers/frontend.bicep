targetScope = 'resourceGroup'

param imageName string
param tagName string
param containerRegistryName string 
param location string = resourceGroup().location
param acaEnvName string 
param uamiName string
param appInsightName string

resource acaEnvironment 'Microsoft.App/managedEnvironments@2022-11-01-preview'  existing = {   name: acaEnvName }
resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = { name: uamiName }
resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = { name: appInsightName }

//var revisionSUffix = substring(tagName, 0, 10)

module frontendApp 'modules/http-containerapp.bicep' = {
  name: imageName
  params: {    
    location: location
    containerAppName: imageName
    environmentName: acaEnvironment.name   
    revisionMode: 'Multiple'    
    trafficDistribution: [         
      {           
          revisionName: 'PREV'
          weight: 80
      }
      {
          revisionName: 'NEXT'
          label: 'latest'
          weight: 20
      }
    ]
    //revisionSuffix: revisionSUffix
    revisionSuffix: ${tagName}
    hasIdentity: true
    userAssignedIdentityName: uami.name
    containerImage: '${containerRegistryName}.azurecr.io/${imageName}:${tagName}'
    containerRegistry: '${containerRegistryName}.azurecr.io'
    isPrivateRegistry: true
    containerRegistryUsername: ''
    registryPassword: ''    
    useManagedIdentityForImagePull: true
    containerPort: 80
    enableIngress: true
    isExternalIngress: true
    minReplicas: 1
  }
}