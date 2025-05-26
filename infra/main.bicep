targetScope = 'resourceGroup'

// Parameters
@description('The location for all resources')
param location string = resourceGroup().location

@description('The name prefix for all resources')
param namePrefix string = 'cosmicworks'

// Variables
var uniqueSuffix = uniqueString(resourceGroup().id)

// Module imports
module identity './modules/identity.bicep' = {
  name: 'identityDeploy'
  params: {
    location: location
    namePrefix: namePrefix
    uniqueSuffix: uniqueSuffix
  }
}

module cosmosDb './modules/cosmos.bicep' = {
  name: 'cosmosDbDeploy'
  params: {
    location: location
    namePrefix: namePrefix
    uniqueSuffix: uniqueSuffix
    managedIdentityPrincipalId: identity.outputs.managedIdentityPrincipalId
    currentUserPrincipalId: identity.outputs.currentUserPrincipalId
  }
}

// Outputs
output AZURE_COSMOSDB_ENDPOINT string = cosmosDb.outputs.cosmosDbEndpoint