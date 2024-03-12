
param apimServiceName string
param envrionmentName string
param containerAppName string
param productName string
param apiName string
param backendHostKeyName string = 'containerappbackendhostname'

resource environment 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: envrionmentName  
}

resource neptuneContainerApp 'Microsoft.App/containerApps@2022-03-01' existing = {
  name: containerAppName  
}

resource apiManagementService 'Microsoft.ApiManagement/service@2023-03-01-preview' existing = {
  name: apimServiceName  
}


resource nameValueEntryForBackendHost 'Microsoft.ApiManagement/service/namedValues@2023-03-01-preview' = {
  name: backendHostKeyName
  parent: apiManagementService
  properties: {
    displayName: backendHostKeyName
    secret: false
    value: neptuneContainerApp.properties.configuration.ingress.fqdn
  }
  dependsOn: [
    neptuneContainerApp
  ]
}

module neptuneApiVersionSet 'neptune-product/versionSets/neptune-version-set.bicep' = {
  name: 'neptune-version-set'
  params: {
    name: 'neptune-version-set'
    apimServiceName: apimServiceName
    description: 'Version set for Neptune API'
    versionHeaderName: 'api-version'    
  }
  dependsOn: [
    apiManagementService
  ]
}

module complianceApiVersionSet 'neptune-product/versionSets/compliance-version-set.bicep' = {
  name: 'neptune-version-set'
  params: {
    name: 'compliance-version-set'
    apimServiceName: apimServiceName
    description: 'Version set for Neptune API'
    versionHeaderName: 'api-version'    
  }
  dependsOn: [
    apiManagementService
  ]
}

module neptuneProducts 'neptune-product/neptune-product.bicep' = {
  name: productName
  params: {
    apimServiceName: apimServiceName
    productName: productName
    apiName: apiName
    serviceUrl: 'https://${environment.properties.staticIp}/'
    versionSetId: neptuneApiVersionSet.outputs.apiVersionSetId
  }
  dependsOn: [
    environment
    neptuneContainerApp
  ]
}

module complianceProducts 'neptune-product/compliance-product.bicep' = {
  name: productName
  params: {
    apimServiceName: apimServiceName
    productName: productName
    apiName: apiName
    serviceUrl: 'https://${environment.properties.staticIp}/'
    versionSetId: neptuneApiVersionSet.outputs.apiVersionSetId
  }
  dependsOn: [
    environment
    neptuneContainerApp
  ]
}

output staticIp string = environment.properties.staticIp
output neptuneApiBackendFqdn string = neptuneContainerApp.properties.configuration.ingress.fqdn
