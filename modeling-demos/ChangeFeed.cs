using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using models;
using cosmos_management;

namespace modeling_demos
{
    public class ChangeFeed
    {
        private ChangeFeedProcessor? changeFeedProcessor;
        private CosmosClient _cosmosClient;
        private Container _monitoredContainer;
        private Container _outputContainer;
        private Container _leasesContainer;

        public ChangeFeed(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;

            _monitoredContainer = _cosmosClient.GetContainer("database-v3", "productCategory");
            _outputContainer = _cosmosClient.GetContainer("database-v3", "product");
            _leasesContainer = _cosmosClient.GetContainer("database-v3", "leases");

        }

        //Start Cosmos DB Change Feed Processor
        public async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync()
        {
            // Create an instance of the Change Feed Processor
            ChangeFeedProcessor changeFeedProcessor = _monitoredContainer
                .GetChangeFeedProcessorBuilder<ProductCategory>("UpdateProductCategoryChanges", HandleChangesAsync)
                .WithInstanceName("UpdateProductCategoryChanges")
                .WithLeaseContainer(_leasesContainer)
                .Build();

            // Start the Change Feed Processor
            await changeFeedProcessor.StartAsync();

            return changeFeedProcessor;
        }

        //Implement the HandleChangesAsync method
        private async Task HandleChangesAsync(IReadOnlyCollection<ProductCategory> changes, CancellationToken cancellationToken)
        {

            Console.WriteLine(changes.Count + " Change(s) Received");

            List<Task> tasks = new List<Task>();

            //Fetch each change to productCategory container
            foreach (ProductCategory item in changes)
            {
                string categoryId = item.id;
                string categoryName = item.name;

                tasks.Add(UpdateProductCategoryName(categoryId, categoryName));
            }

            await Task.WhenAll(tasks);

        }


        private async Task UpdateProductCategoryName(string categoryId, string categoryName)
        {
            //query all products for the category
            string sql = $"SELECT * FROM c WHERE c.categoryId = @categoryId";

            FeedIterator<Product> resultSet = _outputContainer.GetItemQueryIterator<Product>(
                new QueryDefinition(sql)
                .WithParameter("@categoryId", categoryId),
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
                    await _outputContainer.ReplaceItemAsync(
                        partitionKey: new PartitionKey(categoryId),
                        id: product.id,
                        item: product);
                }

                Console.WriteLine($"Updated {productCount} products with updated category name '{categoryName}'");
            }
        }
    }
}
