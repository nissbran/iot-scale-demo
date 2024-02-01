param name string
param location string

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: 'acriot${name}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

output acrName string = acr.name
