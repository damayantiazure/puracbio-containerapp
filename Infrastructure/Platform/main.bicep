targetScope = 'resourceGroup'

param location string = resourceGroup().location
param uamiName string 
param vnetName string
param containerRegistryName string
param keyvaultName string
param logAnalyticsName string
param appInsightName string
param acaEnvName string

param apimServiceName string
param publicIpAddressName string
param publisherEmail string
param publisherName string
param sku string
param skuCount int

module virtualNetwork 'modules/virtual-network.bicep' = {
  name: vnetName
  params: {
    vnetName: vnetName
    location: location    
  }
}

module uami 'modules/identity.bicep' = {
  name: uamiName
  params: {
    uamiName: uamiName
    location: location
  }
}

module containerRegistry  'modules/registry.bicep' = {
  name: containerRegistryName
  params: {
    location: location
    registryName: containerRegistryName
    skuName: 'Basic'
    userAssignedIdentityPrincipalId: uami.outputs.principalId
    adminUserEnabled: false
  }
}

module keyvault 'modules/keyvault.bicep' = {
  name: keyvaultName
  params: {
    keyVaultName: keyvaultName
    objectId: uami.outputs.principalId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    keysPermissions: [
      'get'
      'list'
    ]
    secretsPermissions: [
      'get'
      'list'
    ]
    location: location
    skuName: 'standard'  
  }
}

module logAnalytics 'modules/log-analytics.bicep' = {
  name: logAnalyticsName
  params: {
    logAnalyticsName: logAnalyticsName
    localtion: location
  }
}

module appInsights 'modules/app-insights.bicep' = {
  name: appInsightName
  params: {
    appInsightName: appInsightName
    location: location
    laWorkspaceId: logAnalytics.outputs.laWorkspaceId
  }
}


module acaEnvironment 'modules/environment.bicep' = {
  name: acaEnvName
  params: {
    appInsightKey: appInsights.outputs.InstrumentationKey
    infrastructureSubnetId: virtualNetwork.outputs.defaultSubnetId
    location: location
    envrionmentName: acaEnvName
    laWorkspaceName: logAnalyticsName
  }
}

module apimService 'modules/apim.bicep' = {
  name: apimServiceName
  params: {
    apimServiceName: apimServiceName
    location: location
    sku: sku
    skuCount: skuCount
    publisherEmail: publisherEmail
    publisherName: publisherName
    publicIpAddressName: publicIpAddressName
    subnetName: virtualNetwork.outputs.apimSubnetName
    virtualNetworkName: virtualNetwork.name
  }
}

