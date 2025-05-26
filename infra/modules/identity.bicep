// Parameters
param location string
param namePrefix string
param uniqueSuffix string

// Variables
var managedIdentityName = '${namePrefix}-identity-${uniqueSuffix}'

// Get the current Azure deployment principal
var currentPrincipalId = length(az.objectId) > 0 ? az.objectId : '00000000-0000-0000-0000-000000000000'

// Create a user-assigned managed identity for the application
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

// Outputs
output managedIdentityPrincipalId string = managedIdentity.properties.principalId
output managedIdentityId string = managedIdentity.id
output managedIdentityClientId string = managedIdentity.properties.clientId
output currentUserPrincipalId string = currentPrincipalId