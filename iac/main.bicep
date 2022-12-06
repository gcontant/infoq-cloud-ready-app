targetScope = 'subscription'

@description('Location where all resource will be deployed')
param location string = 'centralus'

@description('The Azure Container Registry user name used to retreive the container image')
param dockerRegistryUsername string

@secure()
@description('The Azure Container Registry associated with the user specified')
param dockerRegistryPassword string

@description('The project database user name')
param dbAdminUser string

@secure()
@description('The password used to connect to the project database')
param dbAdminPassword string

var rgName = 'sshs'
var dbServerName = 'sshsdbsrvprodcatalog01'
var dbName = 'sshsdbprodcatalog01'

module rg 'modules/Microsoft.Resources/resourceGroups/deploy.bicep' = {
  name: 'sshs-rg'
  params: {
    name: rgName
    location: location
    enableDefaultTelemetry: false
  }
}

module keyvault 'modules/Microsoft.KeyVault/vaults/deploy.bicep' = {
  name: 'sshs-kv'
  scope: resourceGroup(rgName)
  dependsOn: [ rg ]
  params: {
    name: 'sshskeyvault01'
    location: location
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false
    vaultSku: 'standard'
    enableVaultForDiskEncryption: false
  }
}

module acr 'modules/Microsoft.ContainerRegistry/registries/deploy.bicep' = {
  name: 'sshs-acr'
  scope: resourceGroup(rgName)
  dependsOn: [ rg ]
  params: {
    name: 'sshsconrgs01'
    location: location
    acrSku: 'Basic'
    acrAdminUserEnabled: true
  }
}

module appServicePlan 'modules/Microsoft.Web/serverfarms/deploy.bicep' = {
  name: 'sshs-appSvcPlan'
  scope: resourceGroup(rgName)
  dependsOn: [ rg ]
  params: {
    name: 'sshsappsrvpln01'
    location: location
    sku: {
      name: 'F1'
    }
    serverOS: 'Linux'
  }
}

module appService 'modules/Microsoft.Web/sites/deploy.bicep' = {
  name: 'sshs-appSvc'
  scope: resourceGroup(rgName)
  params: {
    name: 'sshsappsrvcat01'
    location: location
    kind: 'app'
    systemAssignedIdentity: true
    serverFarmResourceId: appServicePlan.outputs.resourceId
    appSettingsKeyValuePairs: {
      DOCKER_REGISTRY_SERVER_URL: acr.outputs.location
      DOCKER_REGISTRY_SERVER_USERNAME: dockerRegistryUsername
      DOCKER_REGISTRY_SERVER_PASSWORD: dockerRegistryPassword
      WEBSITES_ENABLE_APP_SERVICE_STORAGE: false
    }
  }
}

module kvAccessPolicy 'modules/Microsoft.KeyVault/vaults/accessPolicies/deploy.bicep' = {
  scope: resourceGroup(rgName)
  name: 'keyvault-sshsappsrvcat01-accesspolicy'
  params: {
    keyVaultName: keyvault.outputs.name
    accessPolicies: [
      {
        objectId: appService.outputs.systemAssignedPrincipalId
        permissions: {
          secrets: [
            'Get'
            'List'
          ]
        }
      }
    ]
  }
}

module postgresServer 'modules/Microsoft.DBforPostgreSQL/flexibleServers/deploy.bicep' = {
  scope: resourceGroup(rgName)
  dependsOn: [ rg ]
  name: 'postgresqlServer'
  params: {
    location: location
    administratorLogin: dbAdminUser
    administratorLoginPassword: dbAdminPassword
    name: dbServerName
    skuName: 'Standard_D4ds_v4'
    tier: 'GeneralPurpose'
    highAvailability: 'Disabled'
    version: '11'
    databases: [
      {
        name: dbName
      }
    ]
    firewallRules: [
      {
        name: 'AzureFirewallRule'
        startIpAddress: '0.0.0.0'
        endIpAddress: '0.0.0.0'
      }
    ]
  }
}

module kvPostgresSecret 'modules/Microsoft.KeyVault/vaults/secrets/deploy.bicep' = {
  scope: resourceGroup(rgName)
  name: 'kvPostgresSecret'
  params: {
    keyVaultName: keyvault.outputs.name
    name: 'ConnectionStrings--ProductCatalogDbPgSqlConnection'
    value: 'Database=${dbName};Server=${dbServerName}.postgres.database.azure.com;UserId=${dbAdminUser};Password=${dbAdminPassword}'
  }
}

module servicebus 'modules/Microsoft.ServiceBus/namespaces/deploy.bicep' ={
  name: 'sshsBus'
  scope: resourceGroup(rgName)
  params:{
    name: 'sshssrvbusnmps01'
    location: location
  }
}

module sbQueue 'modules/Microsoft.ServiceBus/namespaces/queues/deploy.bicep' ={
  name: 'sshssbqueue'
  scope: resourceGroup(rgName)
  params:{
    name: 'sshssrvbusqueu01'
    namespaceName: servicebus.outputs.name
  }  
}


module kvServiceBusSecret 'modules/Microsoft.KeyVault/vaults/secrets/deploy.bicep' = {
  scope: resourceGroup(rgName)
  name: 'kvServiceBusSecret'
  params: {
    keyVaultName: keyvault.outputs.name
    name: 'ConnectionStrings--ServiceBus'
    value: listKeys('sshssrvbusqueu01','2021-06-01-preview').primaryConnectionString
  }
}
