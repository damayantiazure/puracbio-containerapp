using 'products.bicep'

var appname = readEnvironmentVariable('APP_NAME')
var appEnv = readEnvironmentVariable('APP_ENV')

param apimServiceName = '${appname}apim${appEnv}'
param envrionmentName = '${appname}-appenv-${appEnv}'
param containerAppName = 'neptune-webapi'
param productName = '${appname}-product'
param apiName = '${appname}-api'
param azureDevOpsEndpoint = 'https://dev.azure.com/moim/_apis/connectionData'
param azureDevOpsEndpointKeyName = 'azuredevopsendpoint'
param backendHostKeyName = 'containerappbackendhostname'
