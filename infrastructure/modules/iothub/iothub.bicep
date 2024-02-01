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

var consumerGroupName = '${iotHubName}/events/cg1'

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
