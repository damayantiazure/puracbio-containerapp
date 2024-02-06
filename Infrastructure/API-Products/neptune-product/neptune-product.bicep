
param apimServiceName string
param serviceUrl string
param productName string 
param apiName string 
param versionSetId string

resource neptuneProduct 'Microsoft.ApiManagement/service/products@2023-03-01-preview' = {
  name: '${apimServiceName}/${productName}'
  properties: {
    displayName: '${productName} description'
    description: '${productName} proxied to ACA API online'
    terms: '${productName} Terms'
    subscriptionRequired: false
    // approvalRequired: false
    // subscriptionsLimit: 1
    state: 'published'    
  }
}



module neptuneWebApi 'apis/neptune-webapi.bicep' = {
  name: apiName
  dependsOn: [
    neptuneProduct
  ]
  params: {
    apimServiceName: apimServiceName
    productName: productName
    apiName: apiName
    serviceUrl: serviceUrl
    apiRevision: '1'
    apiRevisionDescription: 'A new revision of the API'
    isCurrent: true
    apiType: 'http'
    description: 'Neptune Web API'
    displayName: 'Neptune Web API'
    apiVersion: 'v1-preview-01'
    apiVersionDescription: 'A preview version of the API'
    apiVersionSetId: versionSetId
  }
}
