using 'main.bicep'

var appname = readEnvironmentVariable('appname')
var appEnv = readEnvironmentVariable('appEnv')

param uamiName = '${appname}-uami-${appEnv}'
param containerRegistryName = '${appname}contregistry${appEnv}'
param keyvaultName = '${appname}keyvault${appEnv}'
param logAnalyticsName = '${appname}-log-analytics-${appEnv}'
param appInsightName = '${appname}-appinsights-${appEnv}'
param acaEnvName = '${appname}-appenv-${appEnv}'

param apimServiceName = '${appname}apim${appEnv}13'
param publisherEmail = 'dbhuyan@microsoft.com'
param publisherName = 'compliancewebapi Inc.'
param sku = 'Premium' // (Premium | Standard | Developer | Basic | Consumption)
param skuCount = 1
param vnetName = '${appname}-vnet-${appEnv}'
param publicIpAddressName = '${appname}-publicip-${appEnv}'
