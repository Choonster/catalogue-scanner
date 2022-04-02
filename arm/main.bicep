// App service plans use a maximum of 22 characters from app name:
// 40 [max length] - 5 [prefix and dashes] - 13 [unique string] = 22

@description('The location of the deployed resources. Must be one of the supported and registered Azure Geo Regions (e.g. australiaeast, westus, etc.).')
param location string = resourceGroup().location

@description('The name of the Azure App Configuration resource')
@minLength(5)
@maxLength(50)
param appConfigurationName string

@description('The SKU of the Azure App Configuration resource. Free can only be used if you don\'t already have another Free instance in your subscription.')
@allowed([
  'Free'
  'Standard'
])
param appConfigurationSku string = 'Standard'

@description('The name of the Log Analytics Workspace')
@minLength(4)
@maxLength(63)
param logAnalyticsWorkspaceName string

@description('The [.NET culture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=net-6.0#culture-names-and-identifiers) for the applications. This setting controls the language used in catalogue digest emails and log messages.')
param localisationCulture string = 'en-AU'

@description('The name of the Function App')
@minLength(2)
@maxLength(60)
param functionAppName string

@description('The name of the Function App\'s App Service Plan')
@minLength(1)
@maxLength(40)
param functionAppPlanName string = 'ASP-${substring(functionAppName, 0, 22)}-${uniqueString(resourceGroup().id, functionAppName)}'

@description('The name of the Storage Account to use for the Function App')
@minLength(3)
@maxLength(24)
param storageAccountName string

@description('The SendGrid API key to use for sending emails')
param sendGridApiKey string

@description('The [CRON expression](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp#ncrontab-expressions) to use for scanning SaleFinder catalogues')
param checkCatalogueFunctionCronExpression string = '0 0 15 * * Tue'

@description('The [CRON expression](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp#ncrontab-expressions) to use for scanning Coles Online')
param colesOnlineCronExpression string = '0 0 15 * * Tue'

@description('The [CRON expression](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp#ncrontab-expressions) to use for scanning Woolworths Online')
param woolworthsOnlineCronExpression string = '0 0 15 * * Tue'

@description('The name of the Configuration UI site')
@minLength(2)
@maxLength(60)
param configurationUiName string

@description('The name of the Configuration UI\'s App Service Plan')
@minLength(1)
@maxLength(40)
param configurationUiPlanName string = 'ASP-${substring(configurationUiName, 0, 22)}-${uniqueString(resourceGroup().id, configurationUiName)}'

@description('The name of the SignalR Service to use for the Configuration UI')
@minLength(3)
@maxLength(63)
param signalRServiceName string

// Shared Resources

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2021-10-01-preview' = {
  name: appConfigurationName
  location: location
  sku: {
    name: appConfigurationSku
  }
}

var appConfigurationConnectionString = appConfiguration.listKeys().value[0].connectionString

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

var logAnalyticsWorkspaceId = logAnalyticsWorkspace.id

// Function App

module functionApp 'function-app.bicep' = {
  name: 'functionApp'
  params: {
    location: location
    localisationCulture: localisationCulture
    storageAccountName: storageAccountName
    colesOnlineCronExpression: colesOnlineCronExpression
    name: functionAppName
    checkCatalogueFunctionCronExpression: checkCatalogueFunctionCronExpression
    sendGridApiKey: sendGridApiKey
    planName: functionAppPlanName
    appConfigurationConnectionString: appConfigurationConnectionString
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    woolworthsOnlineCronExpression: woolworthsOnlineCronExpression
  }
}

// Configuration UI

module configurationUi 'configuration-ui.bicep' = {
  name: 'configurationUi'
  params: {
    apiBaseAddress: functionApp.outputs.apiBaseAddress
    location: location
    localisationCulture: localisationCulture
    name: configurationUiName
    signalRServiceName: signalRServiceName
    appConfigurationConnectionString: appConfigurationConnectionString
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    planName: configurationUiPlanName
  }
}
