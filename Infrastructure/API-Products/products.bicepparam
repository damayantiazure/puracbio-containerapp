using 'products.bicep'

var appname = readEnvironmentVariable('APP_NAME')
var appEnv = readEnvironmentVariable('APP_ENV')
var imageName = readEnvironmentVariable('imageName')

param apimServiceName = '${appname}apim${appEnv}13'
param envrionmentName = '${appname}-appenv-${appEnv}'
param containerAppName = '${imageName}'
param productName = '${appname}-product'
param apiName = '${appname}-api'
param backendHostKeyName = 'containerappbackendhostname'
