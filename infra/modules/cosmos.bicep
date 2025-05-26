// Parameters
param location string
param namePrefix string
param uniqueSuffix string
param managedIdentityPrincipalId string
param currentUserPrincipalId string

// Variables
var cosmosDBAccountName = '${namePrefix}-db-${uniqueSuffix}'
var cosmosDataContributorRoleId = '00000000-8000-0000-0000-000000000002' // Cosmos DB Built-in Data Contributor Role ID

// Resources
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosDBAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    enableFreeTier: false
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

// Database v1
resource databaseV1 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosAccount
  name: 'database-v1'
  properties: {
    resource: {
      id: 'database-v1'
    }
  }
}

// Database v1 Containers
resource customerV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'customer'
  properties: {
    resource: {
      id: 'customer'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource customerAddressV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'customerAddress'
  properties: {
    resource: {
      id: 'customerAddress'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource customerPasswordV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'customerPassword'
  properties: {
    resource: {
      id: 'customerPassword'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'product'
  properties: {
    resource: {
      id: 'product'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productCategoryV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'productCategory'
  properties: {
    resource: {
      id: 'productCategory'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productTagV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'productTag'
  properties: {
    resource: {
      id: 'productTag'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productTagsV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'productTags'
  properties: {
    resource: {
      id: 'productTags'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource salesOrderV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'salesOrder'
  properties: {
    resource: {
      id: 'salesOrder'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource salesOrderDetailV1Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV1
  name: 'salesOrderDetail'
  properties: {
    resource: {
      id: 'salesOrderDetail'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

// Database v2
resource databaseV2 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosAccount
  name: 'database-v2'
  properties: {
    resource: {
      id: 'database-v2'
    }
  }
}

// Database v2 Containers
resource customerV2Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV2
  name: 'customer'
  properties: {
    resource: {
      id: 'customer'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productV2Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV2
  name: 'product'
  properties: {
    resource: {
      id: 'product'
      partitionKey: {
        paths: [
          '/categoryId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productCategoryV2Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV2
  name: 'productCategory'
  properties: {
    resource: {
      id: 'productCategory'
      partitionKey: {
        paths: [
          '/type'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productTagV2Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV2
  name: 'productTag'
  properties: {
    resource: {
      id: 'productTag'
      partitionKey: {
        paths: [
          '/type'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource salesOrderV2Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV2
  name: 'salesOrder'
  properties: {
    resource: {
      id: 'salesOrder'
      partitionKey: {
        paths: [
          '/customerId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

// Database v3
resource databaseV3 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosAccount
  name: 'database-v3'
  properties: {
    resource: {
      id: 'database-v3'
    }
  }
}

// Database v3 Containers
resource customerV3Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV3
  name: 'customer'
  properties: {
    resource: {
      id: 'customer'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productV3Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV3
  name: 'product'
  properties: {
    resource: {
      id: 'product'
      partitionKey: {
        paths: [
          '/categoryId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productCategoryV3Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV3
  name: 'productCategory'
  properties: {
    resource: {
      id: 'productCategory'
      partitionKey: {
        paths: [
          '/type'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productTagV3Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV3
  name: 'productTag'
  properties: {
    resource: {
      id: 'productTag'
      partitionKey: {
        paths: [
          '/type'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource salesOrderV3Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV3
  name: 'salesOrder'
  properties: {
    resource: {
      id: 'salesOrder'
      partitionKey: {
        paths: [
          '/customerId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

// Database v4
resource databaseV4 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosAccount
  name: 'database-v4'
  properties: {
    resource: {
      id: 'database-v4'
    }
  }
}

// Database v4 Containers
resource customerV4Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV4
  name: 'customer'
  properties: {
    resource: {
      id: 'customer'
      partitionKey: {
        paths: [
          '/customerId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productV4Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV4
  name: 'product'
  properties: {
    resource: {
      id: 'product'
      partitionKey: {
        paths: [
          '/categoryId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource productMetaV4Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV4
  name: 'productMeta'
  properties: {
    resource: {
      id: 'productMeta'
      partitionKey: {
        paths: [
          '/type'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource salesByCategoryV4Container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: databaseV4
  name: 'salesByCategory'
  properties: {
    resource: {
      id: 'salesByCategory'
      partitionKey: {
        paths: [
          '/categoryId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

// Role assignments
resource managedIdentityRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-04-15' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, managedIdentityPrincipalId, cosmosDataContributorRoleId)
  properties: {
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions', cosmosAccount.name, cosmosDataContributorRoleId)
    scope: cosmosAccount.id
  }
}

resource currentUserRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-04-15' = if (currentUserPrincipalId != '00000000-0000-0000-0000-000000000000') {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, currentUserPrincipalId, cosmosDataContributorRoleId)
  properties: {
    principalId: currentUserPrincipalId
    principalType: 'User'
    roleDefinitionId: subscriptionResourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions', cosmosAccount.name, cosmosDataContributorRoleId)
    scope: cosmosAccount.id
  }
}

// Outputs
output cosmosDbEndpoint string = cosmosAccount.properties.documentEndpoint
output cosmosDbName string = cosmosAccount.name