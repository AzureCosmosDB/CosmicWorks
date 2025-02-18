metadata description = 'Provisions Azure Cosmos DB for NoSQL for the Cosmic Works sample.'

targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention.')
param environmentName string

@minLength(1)
@description('Primary location for all resources.')
param location string

@description('Id of the principal to assign database and application roles.')
param deploymentUserPrincipalId string = ''

// serviceName is used as value for the tag (azd-service-name) azd uses to identify deployment host
param serviceName string = 'cosmic-works'

var resourceToken = toLower(uniqueString(resourceGroup().id, environmentName))
var tags = {
  'azd-service-name': serviceName
  'azd-env-name': environmentName
  repo: 'https://github.com/AzureCosmosDB/CosmicWorks'
}

module managedIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.4.0' = {
  name: 'user-assigned-identity'
  params: {
    name: 'managed-identity-${resourceToken}'
    location: location
    tags: tags
  }
}

module cosmosDbAccount 'br/public:avm/res/document-db/database-account:0.8.1' = {
  name: 'cosmos-db-account'
  params: {
    name: 'cosmic-works-${resourceToken}'
    location: location
    locations: [
      {
        failoverPriority: 0
        locationName: location
        isZoneRedundant: false
      }
    ]
    tags: tags
    disableKeyBasedMetadataWriteAccess: true
    disableLocalAuth: true
    networkRestrictions: {
      publicNetworkAccess: 'Enabled'
      ipRules: []
      virtualNetworkRules: []
    }
    capabilitiesToAdd: [
      'EnableServerless'
    ]
    sqlRoleDefinitions: [
      {
        name: 'nosql-data-plane-contributor'
        dataAction: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
        ]
      }
    ]
    sqlRoleAssignmentsPrincipalIds: union(
      [
        managedIdentity.outputs.principalId
      ],
      !empty(deploymentUserPrincipalId) ? [deploymentUserPrincipalId] : []
    )
    sqlDatabases: [
      {
        name: 'database-v1'
        containers: [
          {
            name: 'customer'
            paths: [
              '/id'
            ]
          }
          {
            name: 'customerAddress'
            paths: [
              '/id'
            ]
          }
          {
            name: 'customerPassword'
            paths: [
              '/id'
            ]
          }
          {
            name: 'product'
            paths: [
              '/id'
            ]
          }
          {
            name: 'productCategory'
            paths: [
              '/id'
            ]
          }
          {
            name: 'productTag'
            paths: [
              '/id'
            ]
          }
          {
            name: 'productTags'
            paths: [
              '/id'
            ]
          }
          {
            name: 'salesOrder'
            paths: [
              '/id'
            ]
          }
          {
            name: 'salesOrderDetail'
            paths: [
              '/id'
            ]
          }
        ]
      }
      {
        name: 'database-v2'
        containers: [
          {
            name: 'customer'
            paths: [
              '/id'
            ]
          }
          {
            name: 'product'
            paths: [
              '/categoryId'
            ]
          }
          {
            name: 'productCategory'
            paths: [
              '/type'
            ]
          }
          {
            name: 'productTag'
            paths: [
              '/type'
            ]
          }
          {
            name: 'salesOrder'
            paths: [
              '/customerId'
            ]
          }
        ]
      }
      {
        name: 'database-v3'
        containers: [
          {
            name: 'customer'
            paths: [
              '/id'
            ]
          }
          {
            name: 'product'
            paths: [
              '/categoryId'
            ]
          }
          {
            name: 'productCategory'
            paths: [
              '/type'
            ]
          }
          {
            name: 'productTag'
            paths: [
              '/type'
            ]
          }
          {
            name: 'salesOrder'
            paths: [
              '/customerId'
            ]
          }
          {
            name: 'leases'
            paths: [
              '/id'
            ]
          }
        ]
      }
      {
        name: 'database-v4'
        containers: [
          {
            name: 'customer'
            paths: [
              '/customerId'
            ]
          }
          {
            name: 'product'
            paths: [
              '/categoryId'
            ]
          }
          {
            name: 'productMeta'
            paths: [
              '/type'
            ]
          }
          {
            name: 'salesByCategory'
            paths: [
              '/categoryId'
            ]
          }
        ]
      }
    ]
  }
}

// Azure Cosmos DB outputs
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP string = resourceGroup().name
output LOCATION string = cosmosDbAccount.outputs.location
output ACCOUNT_NAME string = cosmosDbAccount.outputs.name
output ACCOUNT_ENDPOINT string = cosmosDbAccount.outputs.endpoint
