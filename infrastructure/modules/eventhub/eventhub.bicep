param prefix string
param location string

resource eventHubNs 'Microsoft.EventHub/namespaces@2021-11-01' = {
  name: '${prefix}-eh-ns'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    isAutoInflateEnabled: false
    maximumThroughputUnits: 0
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2021-11-01' = {
  parent: eventHubNs
  name: 'iot-events'
  properties: {
    messageRetentionInDays: 7
    partitionCount: 30
  }
}

resource eventHubMessageRouter 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2021-11-01' = {
  parent: eventHub
  name: 'message-router'
}

resource eventHubSharedKey 'Microsoft.EventHub/namespaces/authorizationRules@2021-11-01' = {
  name: 'string'
  parent: eventHubNs
  properties: {
    rights: [ 'Send', 'Listen', 'Manage' ]
  }
}

