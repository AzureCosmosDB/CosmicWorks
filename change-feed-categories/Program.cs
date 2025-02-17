using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using models;
using cosmos_management;


namespace ChangeFeedConsole
{
    class Program
    {
        //=================================================================
        //Load secrets
        private static IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appSettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>();

        private static IConfigurationRoot config = builder.Build();

        private static readonly CosmosManagement management = new CosmosManagement(config);

        private static readonly string uri = config["uri"];
        //private static readonly string key = config["key"];
        //private static readonly CosmosClient client = new CosmosClient(uri, key);

        private static readonly CosmosClient client = new CosmosClient(uri, new DefaultAzureCredential());

        private static readonly Database database = client.GetDatabase("database-v3");
        private static readonly Container productCategoryContainer = database.GetContainer("productCategory");
        private static readonly Container productContainer = database.GetContainer("product");

        static async Task Main(string[] args)
        {

            await management.CreateOrUpdateCosmosDBContainer("database-v3", "leases", "/id");
            Container leaseContainer = database.GetContainer("leases");

            //ContainerProperties leaseContainerProperties = new ContainerProperties("consoleLeases", "/id");
            //Container leaseContainer = await database.CreateContainerIfNotExistsAsync(leaseContainerProperties, throughput: 400);
                        

            var builder = productCategoryContainer.GetChangeFeedProcessorBuilder("ProductCategoryProcessor",
                async (IReadOnlyCollection<ProductCategory> input, CancellationToken cancellationToken) => 
                {
                    Console.WriteLine(input.Count + " Change(s) Received");

                    List<Task> tasks = new List<Task>();

                    //Fetch each change to productCategory container
                    foreach (ProductCategory item in input)
                    {
                        string categoryId = item.id;
                        string categoryName = item.name;

                        tasks.Add(UpdateProductCategoryName(categoryId, categoryName));
                    }

                    await Task.WhenAll(tasks);
                });

            var processor = builder
                .WithInstanceName("ChangeFeedProductCategories")
                .WithLeaseContainer(leaseContainer)
                .Build();

            await processor.StartAsync();
            Console.WriteLine("Started Change Feed Processor");
            Console.WriteLine("Press any key to stop the processor...");

            Console.ReadKey();

            Console.WriteLine("Stopping Change Feed Processor");

            await processor.StopAsync();
        }

        private static async Task UpdateProductCategoryName(string categoryId, string categoryName)
        {
            //query all products for the category
            string sql = $"SELECT * FROM c WHERE c.categoryId = @categoryId";

            FeedIterator<Product> resultSet = productContainer.GetItemQueryIterator<Product>(
                new QueryDefinition(sql)
                .WithParameter("@categoryId",categoryId), 
                requestOptions: new QueryRequestOptions 
                { 
                    PartitionKey = new PartitionKey(categoryId)
                });

            int productCount = 0;

            //Loop through all pages
            while (resultSet.HasMoreResults)
            {
                FeedResponse<Product> response = await resultSet.ReadNextAsync();

                //Loop through all products
                foreach (Product product in response)
                {
                    productCount++;
                    //update category name for product
                    product.categoryName = categoryName;
                    
                    //write the update back to product container
                    await productContainer.ReplaceItemAsync(
                        partitionKey: new PartitionKey(categoryId),
                        id: product.id,
                        item: product);
                }

                Console.WriteLine($"Updated {productCount} products with updated category name '{categoryName}'");
            }
        }
    }
}