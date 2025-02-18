using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CosmicWorks
{
    public class CosmosManagement
    {
        private readonly string _subscriptionId = "your-subscription-id";
        private readonly string _resourceGroupName = "your-resource-group-name";
        private readonly string _location = "East US";
        private readonly string _accountName = "your-cosmosdb-account-name";
        private readonly TokenCredential _credential;
        private readonly ArmClient _armClient;


        public CosmosManagement(IConfiguration config)
        {
            _subscriptionId = config["SUBSCRIPTION_ID"]!;
            _resourceGroupName = config["RESOURCE_GROUP"]!;
            _accountName = config["ACCOUNT_NAME"]!;
            _location = config["LOCATION"]!;

            _credential = new DefaultAzureCredential();
            _armClient = new ArmClient(_credential);
        }

        public async Task DeleteAllCosmosDBDatabaes()
        {
            //Get the account
            ResourceIdentifier resourceId = CosmosDBAccountResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName, _accountName);
            CosmosDBAccountResource account = _armClient.GetCosmosDBAccountResource(resourceId);
            
            //Get the databases collection
            CosmosDBSqlDatabaseCollection databases = account.GetCosmosDBSqlDatabases();
            
            //Get all databases
            AsyncPageable<CosmosDBSqlDatabaseResource> allDatabases = databases.GetAllAsync();

            
            //Delete all databases
            await foreach (CosmosDBSqlDatabaseResource database in allDatabases)
            {
                await database.DeleteAsync(WaitUntil.Completed);
                Console.WriteLine($"Deleted Database: {database.Data.Id}");
            }
        }

        public async Task DeleteCosmosDBDatabase(string databaseName)
        {
            //Get the account
            ResourceIdentifier resourceId = CosmosDBAccountResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName, _accountName);
            CosmosDBAccountResource account = _armClient.GetCosmosDBAccountResource(resourceId);
            
            //Get the databases collection
            CosmosDBSqlDatabaseCollection databases = account.GetCosmosDBSqlDatabases();

            //Get the database
            CosmosDBSqlDatabaseResource database = await databases.GetAsync(databaseName);

            //Delete the database
            await database.DeleteAsync(WaitUntil.Completed);
            Console.WriteLine($"Deleted Database: {databaseName}");
        }

        public async Task CreateOrUpdateCosmosDBDatabase(string databaseName)
        {

            //Database properties
            CosmosDBSqlDatabaseCreateOrUpdateContent properties =
                new CosmosDBSqlDatabaseCreateOrUpdateContent(
                _location,
                new CosmosDBSqlDatabaseResourceInfo(databaseName));


            //Get the account
            ResourceIdentifier resourceId = CosmosDBAccountResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName, _accountName);
            CosmosDBAccountResource account = _armClient.GetCosmosDBAccountResource(resourceId);

            //Get the databases collection
            CosmosDBSqlDatabaseCollection databases = account.GetCosmosDBSqlDatabases();

            //Create or update the database
            ArmOperation<CosmosDBSqlDatabaseResource> response = await databases.CreateOrUpdateAsync(WaitUntil.Completed, databaseName, properties);
            CosmosDBSqlDatabaseResource resource = response.Value;

            Console.WriteLine($"Created new Database: {resource.Data.Name}");

        }

        public async Task CreateOrUpdateCosmosDBContainer(string databaseName, string containerName, string partitionKey)
        {

            // Container properties
            CosmosDBSqlContainerCreateOrUpdateContent properties =
            new CosmosDBSqlContainerCreateOrUpdateContent(
                _location,
                new CosmosDBSqlContainerResourceInfo(containerName)
                {
                    PartitionKey = new CosmosDBContainerPartitionKey()
                    {
                        Paths = { partitionKey },
                        Kind = CosmosDBPartitionKind.Hash,  //Hash for single partition key, MultiHash for hierarchical partition key
                        Version = 2
                    },
                    IndexingPolicy = new CosmosDBIndexingPolicy()
                    {
                        IsAutomatic = true,
                        IndexingMode = CosmosDBIndexingMode.Consistent,
                        IncludedPaths =
                        {
                            new CosmosDBIncludedPath()
                            {
                                Path = "/*"
                            }
                        },
                        ExcludedPaths =
                        {
                            new CosmosDBExcludedPath()
                            {
                                Path = "/\"_etag\"/?"
                            }
                        }
                    }
                    
                }
            );


            //Get the CosmosDB database
            ResourceIdentifier resourceId = CosmosDBSqlDatabaseResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName, _accountName, databaseName);
            CosmosDBSqlDatabaseResource cosmosDBDatabase = _armClient.GetCosmosDBSqlDatabaseResource(resourceId);

            //Get the containers collection
            CosmosDBSqlContainerCollection cosmosContainers = cosmosDBDatabase.GetCosmosDBSqlContainers();

            //Create or update the container
            ArmOperation<CosmosDBSqlContainerResource> response = await cosmosContainers.CreateOrUpdateAsync(WaitUntil.Completed, containerName, properties);
            CosmosDBSqlContainerResource resource = response.Value;

            Console.WriteLine($"Created new Container: {resource.Data.Name}");
        }

        public async Task ApplyCosmosRbacToAccount()
        {
            await CreateOrUpdateRoleAssignment(await GetBuiltInDataContributorRoleDefinitionAsync());
        }

        private async Task CreateOrUpdateRoleAssignment(ResourceIdentifier roleDefintionId)
        {

            //Get the principal ID of the current logged-in user
            Guid? principalId = await GetCurrentUserPrincipalIdAsync();

            //Select the type of role to assign
            //ResourceIdentifier roleDefintionId = await GetBuiltInDataContributorRoleDefinitionAsync();
            //ResourceIdentifier roleDefinitionId = await CreateOrUpdateCustomRoleDefinition();

            //Select the scope of the role permissions
            string assignableScope = GetAssignableScope(Scope.Account);

            //Role assignment properties
            CosmosDBSqlRoleAssignmentCreateOrUpdateContent properties = new CosmosDBSqlRoleAssignmentCreateOrUpdateContent()
            {
                RoleDefinitionId = roleDefintionId,
                Scope = assignableScope,
                PrincipalId = principalId
            };

            //Construct a new role assignment resource
            string roleAssignmentId = Guid.NewGuid().ToString();
            ResourceIdentifier resourceId = CosmosDBSqlRoleAssignmentResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName, _accountName, roleAssignmentId);
            CosmosDBSqlRoleAssignmentResource roleAssignment = _armClient.GetCosmosDBSqlRoleAssignmentResource(resourceId);

            //Update the role assignment with the new properties
            ArmOperation<CosmosDBSqlRoleAssignmentResource> response = await roleAssignment.UpdateAsync(WaitUntil.Completed, properties);
            CosmosDBSqlRoleAssignmentResource resource = response.Value;

            Console.WriteLine($"Created new Role Assignment: {resource.Data.Name}");
        }

        private async Task<Guid?> GetCurrentUserPrincipalIdAsync()
        {
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var token = await _credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Token);

            // Extract 'oid' (Object ID / Principal ID)
            if (jwtToken.Payload.TryGetValue("oid", out var oid))
            {
                string userId = oid.ToString()!;
                Guid principalId = new (userId);
                return principalId;
            }

            return null;

        }

        private async Task<ResourceIdentifier> GetBuiltInDataContributorRoleDefinitionAsync()
        {

            //Built-in roles are predefined roles that are available in Azure Cosmos DB
            //Cosmos DB Built-in Data Contributor role definition ID
            string roleDefinitionId = "00000000-0000-0000-0000-000000000002";

            //Get the role definition
            ResourceIdentifier resourceId = CosmosDBSqlRoleDefinitionResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName, _accountName, roleDefinitionId);
            CosmosDBSqlRoleDefinitionResource roleDefinition = await _armClient.GetCosmosDBSqlRoleDefinitionResource(resourceId).GetAsync();

            return roleDefinition.Id;
        }

        private string GetAssignableScope(Scope scope)
        {
            // Switch statement to set the permission scope
            string scopeString = scope switch
            {
                Scope.Subscription => $"/subscriptions/{_subscriptionId}",
                Scope.ResourceGroup => $"/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}",
                Scope.Account => $"/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{_accountName}",
                //Scope.Database => $"/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{_accountName}/dbs/{_databaseName}",
                //Scope.Container => $"/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{_accountName}/dbs/{_databaseName}/colls/{_containerName}",
                _ => $"/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{_accountName}",
            };
            return scopeString;
        }

        private enum Scope
        {
            Subscription,
            ResourceGroup,
            Account,
            Database,
            Container
        }

    }
}
