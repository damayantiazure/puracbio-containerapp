

metadata name = 'API Management Service API Version Sets'
metadata description = 'This module deploys an API Management Service API Version Set.'
metadata owner = 'Azure/module-maintainers'

param apimServiceName string
param name string = 'default'
param description string = ''

@allowed([ 
  'Segment'
  'Query'
  'Header'
])
param versioningScheme string = 'Header' 
param versionQueryName string = 'api-version'
param versionHeaderName string = 'api-version'

resource service 'Microsoft.ApiManagement/service@2021-08-01' existing = {
  name: apimServiceName
}

resource apiVersionSet 'Microsoft.ApiManagement/service/apiVersionSets@2021-08-01' = {
  name: name
  parent: service
  properties: {
    displayName: name
    description: description
    versioningScheme: versioningScheme
    versionQueryName: versionQueryName
    versionHeaderName: versionHeaderName
  }
}


output apiVersionSetId string = apiVersionSet.id
output name string = apiVersionSet.name
output resourceGroupName string = resourceGroup().name
