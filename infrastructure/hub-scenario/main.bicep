targetScope = 'subscription'

param rgDemo string = 'rg-iot-demo'
param prefix string
param location_a string
param location_b string

resource rg_demo 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: rgDemo
  location: location_a
}

module event_hub '../modules/eventhub/eventhub.bicep' = {
  name: 'event_hub'
  scope: rg_demo
  params: {
    location: location_a
    prefix: prefix
  }
}

module iot_hub_a '../modules/iothub/iothub.bicep' = {
  name: 'iot_hub_a'
  scope: rg_demo
  params: {
    location: location_a
    iotHubName: '${prefix}-iot-hub-a'
    eventhubEndpointName: event_hub.outputs.eventHubName
  }
  dependsOn: [
    event_hub
  ]
}

module iot_hub_b '../modules/iothub/iothub.bicep' = {
  name: 'iot_hub_b'
  scope: rg_demo
  params: {
    location: location_b
    iotHubName: '${prefix}-iot-hub-b'
    eventhubEndpointName: event_hub.outputs.eventHubName
  }
  dependsOn: [
    event_hub
  ]
}

module dps '../modules/dps/dps.bicep' = {
  scope: rg_demo
  name: 'dps'
  params: {
    location: location_b
    iotHubAName: iot_hub_a.outputs.iotName
    iotHubBName: iot_hub_b.outputs.iotName
    provisioningServiceName: '${prefix}-dps'
  }
}

// param rgSimDevices string = 'rg-iot-sim-devices'
// resource rg_sim_devices 'Microsoft.Resources/resourceGroups@2023-07-01' = {
//   name: rgSimDevices
//   location: location
// }

// module storage '../modules/storage/storageaccount.bicep' = {
//   scope: rg_demo
//   name: 'storage'
//   params: {
//     location: location
//     prefix: prefix
//   }
// }

// module registry '../modules/containers/registry.bicep' = {
//   scope: rg_sim_devices
//   name: 'registry'
//   params: {
//     location: location
//     name: prefix
//   }
// }

// module aca_env '../modules/containers/aca-env.bicep' = {
//   scope: rg_sim_devices
//   name: 'aca_env'
//   params: {
//     location: location
//     name: prefix
//   }
// }
