targetScope = 'subscription'

@description('Location where all resource will be deployed')
param location string = 'eastus2'

@secure()
@description('The Azure Container Registry user name used to retreive the container image')
param dockerRegistryUsername string

@secure()
@description('The Azure Container Registry associated with the user specified')
param dockerRegistryPassword string

@secure()
@description('The project database user name')
param dbAdminUser string

@secure()
@description('The password used to connect to the project database')
param dbAdminPassword string


module resourceGroup 'modules/Microsoft.Resources/resourceGroups/deploy.bicep' = {
  name: 'sshs-rg'
  params:{
    name: 'sshs'
    location: location
    enableDefaultTelemetry: false
  }
}

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  name: 'sshs'
}

module keyvault 'modules/Microsoft.KeyVault/vaults/deploy.bicep' ={
  name: 'sshs-kv'
  scope: rg
  params: {
    name: 'sshskeyvault01'
    location: location
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false
    vaultSku: 'standard'
    enableVaultForDiskEncryption: false
  }
}

module acr 'modules/Microsoft.ContainerRegistry/registries/deploy.bicep' ={
  name: 'sshs-acr'
  scope: rg
  params:{
    name: 'sshsconrgs01'
    location: location
    acrSku: 'Basic'
    acrAdminUserEnabled: true
  }
}

module appServicePlan 'modules/Microsoft.Web/serverfarms/deploy.bicep' = {
  name: 'sshs-appSvcPlan'
  scope: rg
  params:{
    name: 'sshsappsrvpln01'
    location: location
    sku: {
      name: 'F1'
    }
    serverOS: 'Linux'
  }
}

module appService 'modules/Microsoft.Web/sites/deploy.bicep' ={
  name: 'sshs-appSvc'
  scope: rg
  params:{
    name: 'sshsappsrvcat01'
    location: location
    kind: 'app'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    appSettingsKeyValuePairs:{
      DOCKER_REGISTRY_SERVER_URL: acr.outputs.location
      DOCKER_REGISTRY_SERVER_USERNAME: dockerRegistryUsername
      DOCKER_REGISTRY_SERVER_PASSWORD: dockerRegistryPassword
      WEBSITES_ENABLE_APP_SERVICE_STORAGE: false
    }
  }
}

module kvAccessPolicy 'modules/Microsoft.KeyVault/vaults/accessPolicies/deploy.bicep' = {
  scope: rg
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


module postgresServer 'modules/Microsoft.DBforPostgreSQL/flexibleServers/deploy.bicep' ={
  scope: rg
  name: 'postgresqlServer'
  params: {
    location: location
    administratorLogin: dbAdminUser
    administratorLoginPassword: dbAdminPassword 
    name: 'sshsdbsrvprodcatalog01'
    skuName: 'Standard_B1ms'
    version: '11'
    tier: 'GeneralPurpose'
  }
}

module postgresDatabase 'modules/Microsoft.DBforPostgreSQL/flexibleServers/databases/deploy.bicep' ={
  scope: rg
  name: 'postgresDatabase'
  params: {
    location: location
    flexibleServerName: postgresServer.outputs.name
    name: 'sshsdbprodcatalog01'
  }
}

module postgresFirewallRule 'modules/Microsoft.DBforPostgreSQL/flexibleServers/firewallRules/deploy.bicep' ={
  scope: rg
  name: 'postgresFirewallRule'
  params: {
    endIpAddress: '0.0.0.0'
    flexibleServerName: postgresServer.outputs.name
    name: 'AzureFirewallRule'
    startIpAddress: '0.0.0.0'
  }
}

module kvPostgresSecret 'modules/Microsoft.KeyVault/vaults/secrets/deploy.bicep' = {
  scope: rg
  name: 'kvPostgresSecret'
  params: {
    keyVaultName: keyvault.outputs.name
    name: 'ConnectionStrings--ProductCatalogDbPgSqlConnection'
    value: 'Database=${postgresDatabase.outputs.name};Server=${postgresServer.outputs.name};UserId=${dbAdminUser};Password=${dbAdminPassword}'
  }
}

