using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using models;
using Newtonsoft.Json;

namespace change_feed_category_sales
{
    class Program
    {
        private static IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        private static readonly string uri = config["endpointUri"];
        private static readonly string key = config["primaryKey"];

        private static readonly CosmosClient client = new CosmosClient(uri, key);
        private static readonly Database database = client.GetDatabase("database-v4");
        private static readonly Container customerContainer = database.GetContainer("customer");
        private static readonly Container productContainer = database.GetContainer("product");
        private static readonly Container salesByCategoryContainer = database.GetContainer("salesByCategory");

        static async Task Main(string[] args)
        {
            
            ContainerProperties leaseContainerProperties = new ContainerProperties("categorySalesLeases", "/id");
            Container leaseContainer = await database.CreateContainerIfNotExistsAsync(leaseContainerProperties, throughput: 400);

            var builder = customerContainer.GetChangeFeedProcessorBuilder("SalesByCategoryProcessor",
                async (IReadOnlyCollection<dynamic> input, CancellationToken cancellationToken) =>
                {
                    Console.WriteLine(input.Count + " Change(s) Received");

                    List<Task> tasks = new List<Task>();

                    foreach (dynamic item in input)
                    {
                        if (item.type == "salesOrder")
                        {
                            SalesOrder salesOrder = JsonConvert.DeserializeObject<SalesOrder>(item.ToString());

                            foreach (SalesOrderDetails salesOrderDetails in salesOrder.details)
                            {
                                string sku = salesOrderDetails.sku;
                                int salesAmount = Convert.ToInt32(salesOrderDetails.price * salesOrderDetails.quantity);
                                
                                tasks.Add(UpdateCategorySales(sku, salesAmount));
                            }
                        }
                    }

                    await Task.WhenAll(tasks);
                });

            var processor = builder
                .WithInstanceName("changeFeedSalesByCategory")
                .WithLeaseContainer(leaseContainer)
                //.WithStartTime(DateTime.MinValue.ToUniversalTime())
                .Build();

            await processor.StartAsync();
            Console.WriteLine("Started Change Feed Processor");
            Console.WriteLine("Press any key to stop the processor...");

            Console.ReadKey();

            Console.WriteLine("Stopping Change Feed Processor");

            await processor.StopAsync();
        }

        private static async Task UpdateCategorySales(string sku, int newSales)
        {

            string categoryId = null;
            string categoryName = null;

            string sql = $"SELECT c.categoryId, c.categoryName, 'category' as type FROM c WHERE c.sku = '{sku}'";

            FeedIterator<dynamic> resultSet = productContainer.GetItemQueryIterator<dynamic>(new QueryDefinition(sql));

            while (resultSet.HasMoreResults)
            {
                FeedResponse<dynamic> response = await resultSet.ReadNextAsync();

                foreach (dynamic category in response)
                {
                    categoryId = category.categoryId;
                    categoryName = category.categoryName;
                }
            }

            //Get current sales for category
            CategorySales categorySales = await ReadCategorySales(categoryId);

           if(categorySales!= null)
           { 
                categorySales.totalSales += newSales;
           }
           else
            {   //First time execution, no data in container
                categorySales = new CategorySales();
                categorySales.id = categoryId;
                categorySales.categoryId = categoryId;
                categorySales.categoryName = categoryName;
                categorySales.totalSales = newSales;  
            }

            //update total sales for category
            await salesByCategoryContainer.UpsertItemAsync<CategorySales>(categorySales, new PartitionKey(categoryId));

            Console.WriteLine($"Category: {categorySales.categoryName} updated total sales: {categorySales.totalSales}");
        }

        private static async Task<CategorySales> ReadCategorySales(string categoryId)
        {
            CategorySales categorySales = null;
            try
            {
                ItemResponse<CategorySales> response = await salesByCategoryContainer.ReadItemAsync<CategorySales>(
                    id: categoryId,
                    partitionKey: new PartitionKey(categoryId));

                categorySales = response.Resource;
            }
            catch (CosmosException e)
            {   //First time execution, no data in container
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    categorySales = null;
                }
            }
            return categorySales;
        }
        
    }
}
