@description('Specifies the name of the IoT Hub.')
@minLength(3)
param iotHubName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Specifies the IotHub SKU.')
param skuName string = 'S1'

@description('Specifies the number of provisioned IoT Hub units. Restricted to 1 unit for the F1 SKU. Can be set up to maximum number allowed for subscription.')
@minValue(1)
@maxValue(1)
param capacityUnits int = 1

param evenhubEndpointName string

resource eh 'Microsoft.EventHub/namespaces@2023-01-01-preview' existing = {
  name: evenhubEndpointName
}

resource sharedAccessPolicy 'Microsoft.EventHub/namespaces/authorizationRules@2023-01-01-preview' existing = {
  name: 'iot-hubs-shared-key'
  parent: eh
}

resource iotHub 'Microsoft.Devices/IotHubs@2023-06-30' = {
  name: iotHubName
  location: location
  properties: {
    eventHubEndpoints: {
      events: {
        retentionTimeInDays: 1
        partitionCount: 2
      }
    }
    routing: {
      endpoints: {
        eventHubs: [
          {
            name: 'iot-events'
            connectionString: 'Endpoint=sb://${eh.name}.servicebus.windows.net;SharedAccessKeyName=iot-hubs-shared-key;SharedAccessKey=${sharedAccessPolicy.listKeys().primaryKey};EntityPath=iot-events'
            authenticationType: 'keyBased'
            subscriptionId: subscription().subscriptionId
            resourceGroup: resourceGroup().name
          }
        ]
      }
      routes: [
        {
          name: 'all-telemetry'
          endpointNames: [
            'iot-events'
          ]
          isEnabled: true
          source: 'DeviceMessages'
        }
        {
          name: 'connection-status'
          endpointNames: [
            'iot-events'
          ]
          isEnabled: true
          source: 'DeviceConnectionStateEvents'
        }
      ]
      enrichments: [
        {
          endpointNames: [
            'iot-events'
          ]
          key: 'hub'
          value: '$iothubname'
        }
      ]
    }
    cloudToDevice: {
      defaultTtlAsIso8601: 'PT1H'
      maxDeliveryCount: 10
      feedback: {
        ttlAsIso8601: 'PT1H'
        lockDurationAsIso8601: 'PT60S'
        maxDeliveryCount: 10
      }
    }
    // messagingEndpoints: {
    //   fileNotifications: {
    //     ttlAsIso8601: 'PT1H'
    //     lockDurationAsIso8601: 'PT1M'
    //     maxDeliveryCount: 10
    //   }
    // }
  }
  sku: {
    name: skuName
    capacity: capacityUnits
  }
}

// resource consumerGroup 'Microsoft.Devices/IotHubs/eventHubEndpoints/ConsumerGroups@2023-06-30' = {
//   name: consumerGroupName
//   properties: {
//     name: 'cg1'
//   }
//   dependsOn: [
//     iotHub
//   ]
// }

output iotName string = iotHub.name
