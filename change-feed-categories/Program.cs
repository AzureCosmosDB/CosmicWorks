using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using models;

namespace ChangeFeedConsole
{
    class Program
    {
        private static IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        private static readonly string _endpointUri = config["endpointUri"];
        private static readonly string _primaryKey = config["primaryKey"];
        private static readonly string _databaseId = "database-v3";
        private static readonly string _containerId = "productCategory";
        private static readonly string _destinationContainerId = "product";
        private static CosmosClient _client = new CosmosClient(_endpointUri, _primaryKey);

        static async Task Main(string[] args)
        {
            Database database = _client.GetDatabase(_databaseId);
            Container container = database.GetContainer(_containerId);
            Container destinationContainer = database.GetContainer(_destinationContainerId);

            ContainerProperties leaseContainerProperties = new ContainerProperties("consoleLeases", "/id");
            Container leaseContainer = await database.CreateContainerIfNotExistsAsync(leaseContainerProperties, throughput: 400);

            var builder = container.GetChangeFeedProcessorBuilder("ProductCategoryProcessor",
                async (IReadOnlyCollection<ProductCategory> input, CancellationToken cancellationToken) => 
                {
                    Console.WriteLine(input.Count + " Change(s) Received");

                    List<Task> tasks = new List<Task>();

                    //Fetch each change to productCategory container
                    foreach (ProductCategory item in input)
                    {
                        string categoryId = item.id;
                        string categoryName = item.name;

                        await UpdateProductCategoryName(destinationContainer, categoryId, categoryName);
                    }

                    await Task.WhenAll(tasks);
                });

            var processor = builder
                .WithInstanceName("changeFeedProductCategories")
                .WithLeaseContainer(leaseContainer)
                .Build();

            await processor.StartAsync();
            Console.WriteLine("Started Change Feed Processor");
            Console.WriteLine("Press any key to stop the processor...");

            Console.ReadKey();

            Console.WriteLine("Stopping Change Feed Processor");

            await processor.StopAsync();
        }

        private static async Task UpdateProductCategoryName(Container destinationcontainer, string categoryId, string categoryName)
        {
            string sql = $"SELECT * FROM c WHERE c.categoryId = '{categoryId}'";

            FeedIterator<Product> resultSet = destinationcontainer.GetItemQueryIterator<Product>(
                new QueryDefinition(sql), requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(categoryId) });

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
                    
                    //update product
                    await destinationcontainer.ReplaceItemAsync(
                        partitionKey: new PartitionKey(categoryId),
                        id: product.id,
                        item: product);
                }

                Console.WriteLine($"Updated {productCount} products with updated category name '{categoryName}'");
            }
        }
    }
}