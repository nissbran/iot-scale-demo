param prefix string
param location string

resource eg_mqtt_namespace 'Microsoft.EventGrid/namespaces@2023-12-15-preview' = {
  name: '${prefix}-eg-ns-mqtt'
  location: location
}

resource client_group 'Microsoft.EventGrid/namespaces/clientGroups@2023-12-15-preview' = {
  name: 'sim-client-group'
  parent: eg_mqtt_namespace
}

