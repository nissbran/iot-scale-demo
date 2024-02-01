param name string
param location string

resource loganalytics_workspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'logs${name}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appinsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appinsights${name}'
  kind: 'web'
  location: location
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: loganalytics_workspace.id
  }
}

resource aca_env 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'acaenv${name}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: loganalytics_workspace.properties.customerId
        sharedKey: loganalytics_workspace.listKeys().primarySharedKey
      }
    }
    infrastructureResourceGroup: 'rg-aca-${name}-infra'
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

output acaEnvName string = aca_env.name
