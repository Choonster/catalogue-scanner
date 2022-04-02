param location string

param name string
param planName string

param signalRServiceName string

param localisationCulture string

param apiBaseAddress string
param appConfigurationConnectionString string
param logAnalyticsWorkspaceId string

resource signalRService 'Microsoft.SignalRService/signalR@2021-10-01' = {
  name: signalRServiceName
  location: location
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'true'
      }
    ]
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
    tls: {
      clientCertEnabled: false
    }
  }
  sku: {
    name: 'Free_F1'
    tier: 'Free'
  }
}

resource configurationUiAppInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
  }
}

resource configurationUiPlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: planName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
}

resource configurationUi 'Microsoft.Web/sites@2021-03-01' = {
  name: name
  location: location
  properties: {
    serverFarmId: configurationUiPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: configurationUiAppInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: configurationUiAppInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
        {
          name: 'Azure__SignalR__ConnectionString'
          value: signalRService.listKeys().primaryConnectionString
        }
        {
          name: 'Azure__SignalR__StickyServerMode'
          value: 'Required'
        }
        {
          name: 'CatalogueScannerApi:BaseAddress'
          value: apiBaseAddress
        }
        {
          name: 'ConnectionStrings:AzureAppConfiguration'
          value: appConfigurationConnectionString
        }
        {
          name: 'LocalisationCulture'
          value: localisationCulture
        }
      ]
      phpVersion: 'OFF'
      netFrameworkVersion: 'v6.0'
    }
    clientAffinityEnabled: false
  }
}
