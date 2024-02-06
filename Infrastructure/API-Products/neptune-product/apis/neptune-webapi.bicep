
param apimServiceName string
param productName string = 'neptune-product'
param apiName string = 'neptune-api'
param serviceUrl string
param apiVersion string
param apiVersionSetId string
param apiVersionDescription string = ''

param apiRevision string = '1'
param apiRevisionDescription string = ''
param isCurrent bool = true
param apiType string = 'http'
param description string = 'Neptune Web API'
param displayName string = 'Neptune Web API'

resource neptuneWebApi 'Microsoft.ApiManagement/service/apis@2023-03-01-preview' = {
  name: '${apimServiceName}/${apiName}'  
  properties: {
    apiRevision: apiRevision
    apiRevisionDescription: apiRevisionDescription
    isCurrent: isCurrent
    apiType: apiType
    description: description
    displayName: displayName
    apiVersion: apiVersion
    apiVersionDescription: apiVersionDescription
    apiVersionSetId: apiVersionSetId
    format: 'openapi+json'
    value: loadTextContent('neptune-webapi-swagger.json')
    path: apiName
    subscriptionRequired: false
    serviceUrl: serviceUrl
  }
  
  resource policy 'policies@2023-03-01-preview' = {    
    name: 'policy'
    properties: {
      format: 'rawxml'
      value: loadTextContent('../policies/azdo-authorization-policy.xml')
    }
  }
}

resource neptuneWebApiWithProduct 'Microsoft.ApiManagement/service/products/apis@2023-03-01-preview' = {
  name: '${apimServiceName}/${productName}/${apiName}'
  dependsOn: [
    neptuneWebApi
  ]
}
