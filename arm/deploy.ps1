#Requires -Modules Az.Resources

<#
.SYNOPSIS
    Deploys an instance of Catalogue Scanner and its related resources using Azure Resource Manager.

.NOTES
    This is just a wrapper around the main.bicep template file with named PowerShell parameters for easier command-line use.
#>

param (
    [Parameter(Mandatory)]
    [string]
    $ResourceGroupName,

    [Parameter()]
    [string]
    $Location,

    [Parameter(Mandatory)]
    [string]
    $AppConfigurationName,

    [Parameter()]
    [string]
    $AppConfigurationSku,

    [Parameter(Mandatory)]
    [string]
    $LogAnalyticsWorkspaceName,

    [Parameter()]
    [string]
    $LocalisationCulture,

    [Parameter(Mandatory)]
    [string]
    $FunctionAppName,

    [Parameter()]
    [string]
    $FunctionAppPlanName = '',

    [Parameter(Mandatory)]
    [string]
    $StorageAccountName,

    [Parameter(Mandatory)]
    [string]
    $SendGridApiKey,

    [Parameter()]
    [string]
    $CheckCatalogueFunctionCronExpression,

    [Parameter()]
    [string]
    $ColesOnlineCronExpression,

    [Parameter()]
    [string]
    $WoolworthsOnlineCronExpression,

    [Parameter(Mandatory)]
    [string]
    $ConfigurationUiName,

    [Parameter()]
    [string]
    $ConfigurationUiPlanName,

    [Parameter(Mandatory)]
    [string]
    $SignalRServiceName
)

New-AzResourceGroupDeployment `
    -TemplateFile main.bicep `
    @PSBoundParameters
