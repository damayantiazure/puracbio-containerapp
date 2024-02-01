using 'app.bicep'


var appname = readEnvironmentVariable('APP_NAME')
var appEnv = readEnvironmentVariable('APP_ENV')


param uamiName = '${appname}-uami-${appEnv}'

param imageName = 'neptune-webapi'
param tagName = 'beta2'
param containerRegistryName = '${appname}contregistry${appEnv}'
param acaEnvName = '${appname}-appenv-${appEnv}'


//param appInsightName = '${appname}-appinsights-${appEnv}'
param appInsightName = 'containerapp-demo-api-appinsights-Containerapp-demo-env'

param azureDevOpsOrg = 'damayantibhuyan' 


