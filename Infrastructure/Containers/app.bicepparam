using 'app.bicep'


var appname = readEnvironmentVariable('APP_NAME')
var appEnv = readEnvironmentVariable('APP_ENV')
var tagName = readEnvironmentVariable('tag')
var imageName = readEnvironmentVariable('imageName')


param uamiName = '${appname}-uami-${appEnv}'
param containerRegistryName = '${appname}contregistry${appEnv}'
param acaEnvName = '${appname}-appenv-${appEnv}'
param appInsightName = '${appname}-appinsights-${appEnv}'
param tagName = ${tagName}
param imageName = ${imageName}