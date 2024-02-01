@description('Specify the name of the Iot hub A.')
param iotHubAName string

@description('Specify the name of the Iot hub A.')
param iotHubBName string

@description('Specify the name of the provisioning service.')
param provisioningServiceName string

@description('Specify the location of the resources.')
param location string = resourceGroup().location

@description('The SKU to use for the dps.')
param skuName string = 'S1'

@description('The number of dps units.')
param skuUnits int = 1

var iotHubKey = 'iothubowner'

resource iotHubA 'Microsoft.Devices/IotHubs@2023-06-30' existing = {
  name: iotHubAName
}

resource iotHubB 'Microsoft.Devices/IotHubs@2023-06-30' existing = {
  name: iotHubBName
}

resource provisioningService 'Microsoft.Devices/provisioningServices@2022-12-12' = {
  name: provisioningServiceName
  location: location
  sku: {
    name: skuName
    capacity: skuUnits
  }
  properties: {
    allocationPolicy: 'Static'
    iotHubs: [
      {
        connectionString: 'HostName=${iotHubA.properties.hostName};SharedAccessKeyName=${iotHubKey};SharedAccessKey=${iotHubA.listkeys().value[0].primaryKey}'
        location: location
        applyAllocationPolicy: true
      }
      {
        connectionString: 'HostName=${iotHubB.properties.hostName};SharedAccessKeyName=${iotHubKey};SharedAccessKey=${iotHubB.listkeys().value[0].primaryKey}'
        location: location
        applyAllocationPolicy: true
      }
    ]

  }
}

// resource en 'Microsoft.Devices/provisioningServices/certificates@2022-12-12' = {
//   name: ''
//   properties:{
//     certificate:
//   }
// }
