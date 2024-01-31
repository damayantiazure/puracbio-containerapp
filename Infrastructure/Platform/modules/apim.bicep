
param apimServiceName string
param location string = resourceGroup().location
param publisherEmail string 
param publisherName string 
param sku string
param skuCount int

param virtualNetworkName string
param subnetName string
param publicIpAddressName string

resource vnet 'Microsoft.Network/virtualNetworks@2023-02-01' existing = {
  name: virtualNetworkName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-02-01' existing = {
  parent: vnet
  name: subnetName
}

module publicIpAddress 'ip-address.bicep' = {
  name: publicIpAddressName  
  params: {
    name: publicIpAddressName
    domainNameLabel: '${apimServiceName}-pipdns'
    location: location
  }
}

resource apiManagementService 'Microsoft.ApiManagement/service@2023-03-01-preview' = {
  name: apimServiceName
  location: location
  sku: {
    name: sku
    capacity: skuCount
  }
  properties: {
    publisherName: publisherName
    publisherEmail: publisherEmail
    virtualNetworkType: 'External'
    publicIpAddressId: publicIpAddress.outputs.publicIpAddressId
    virtualNetworkConfiguration: {      
      subnetResourceId: subnet.id
    }
  }
}
