param location string

param name string
param planName string

param storageAccountName string

param localisationCulture string

param sendGridApiKey string
param appConfigurationConnectionString string
param logAnalyticsWorkspaceId string

param checkCatalogueFunctionCronExpression string
param colesOnlineCronExpression string
param woolworthsOnlineCronExpression string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource functionAppAppInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
  }
}

resource functionAppPlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: planName
  location: location
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    capacity: 1
  }
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: name
  location: location
  kind: 'functionapp,linux'
  tags: {
    'hidden-link: /app-insights-resource-id': functionAppAppInsights.id
  }
  properties: {
    serverFarmId: functionAppPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: ''
        }
        {
          name: 'WEBSITE_ENABLE_SYNC_UPDATE_SITE'
          value: 'true'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: functionAppAppInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: functionAppAppInsights.properties.ConnectionString
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'AzureAppConfigurationConnectionString'
          value: appConfigurationConnectionString
        }
        {
          name: 'AzureWebJobsSendGridApiKey'
          value: sendGridApiKey
        }
        {
          name: 'LocalisationCulture'
          value: localisationCulture
        }
        {
          name: 'PLAYWRIGHT_BROWSERS_PATH'
          value: '/home/site/playwright-browsers'
        }
        {
          name: 'TaskHubName'
          value: replace(name, '-', '')
        }
        {
          name: 'CheckCatalogueFunctionCronExpression'
          value: checkCatalogueFunctionCronExpression
        }
        {
          name: 'ColesOnlineCronExpression'
          value: colesOnlineCronExpression
        }
        {
          name: 'WoolworthsOnlineCronExpression'
          value: woolworthsOnlineCronExpression
        }
      ]
    }
  }
}

output apiBaseAddress string = 'https://${functionApp.properties.hostNames[0]}/api/'
