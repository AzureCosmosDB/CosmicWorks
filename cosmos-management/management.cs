using Azure;
using Azure.Identity;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using System.Net;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace cosmos_management
{
    public class Management
    {
        private IConfigurationRoot _config;
        private readonly string _subscriptionId = "your-subscription-id";
        private readonly string _resourceGroupName = "your-resource-group-name";
        private readonly string _location = "East US";
        private readonly string _accountName = "your-cosmosdb-account-name";
        private readonly TokenCredential _credential;
        private readonly ArmClient _armClient;


        public Management(IConfigurationRoot config)
        {
            _config = config;
            _subscriptionId = _config["subscriptionId"]!;
            _resourceGroupName = _config["resourceGroup"]!;
            _accountName = _config["accountName"]!;
            _location = _config["location"]!;

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

            Console.WriteLine($"Created new Database: {resource.Data.Id}");

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

            Console.WriteLine($"Created new Container: {resource.Data.Id}");
        }

    }
}
