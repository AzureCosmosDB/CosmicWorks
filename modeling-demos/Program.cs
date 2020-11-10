using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using models;
using Microsoft.Extensions.Configuration;

namespace modeling_demos
{
    class Program
    {
        private static IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        private static readonly string _endpointUri = config["endpointUri"];
        private static readonly string _primaryKey = config["primaryKey"];
        private static CosmosClient _client = new CosmosClient(_endpointUri, _primaryKey);

        public static async Task Main(string[] args)
        {
            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Cosmos DB Modeling and Partitioning Demos");
                Console.WriteLine($"-----------------------------------------");
                Console.WriteLine($"[a]   Query for single customer");
                Console.WriteLine($"[b]   Point read for single customer");
                Console.WriteLine($"[c]   List all product categories");
                Console.WriteLine($"[d]   Query products by category id");
                Console.WriteLine($"[e]   Update product category name");
                Console.WriteLine($"[f]   Query orders by customer id");
                Console.WriteLine($"[g]   Query for customer and all orders");
                Console.WriteLine($"[h]   Create new order and update order total");
                Console.WriteLine($"[i]   Delete order and update order total");
                Console.WriteLine($"[j]   Query top 10 customers");
                Console.WriteLine($"[x]   Exit");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    Console.Clear();
                    await QueryCustomer();
                }
                else if (result.KeyChar == 'b')
                {
                    Console.Clear();
                    await GetCustomer();
                }
                else if (result.KeyChar == 'c')
                {
                    Console.Clear();
                    await ListAllProductCategories();
                }
                else if (result.KeyChar == 'd')
                {
                    Console.Clear();
                    await QueryProductsByCategoryId();
                }
                else if (result.KeyChar == 'e')
                {
                    Console.Clear();
                    await QueryProductsForCategory();
                    await UpdateProductCategory();
                    await QueryProductsForCategory();
                    await RevertProductCategory();
                }
                else if (result.KeyChar == 'f')
                {
                    Console.Clear();
                    await QuerySalesOrdersByCustomerId();
                }
                else if (result.KeyChar == 'g')
                {
                    Console.Clear();
                    await QueryCustomerAndSalesOrdersByCustomerId();
                }
                else if (result.KeyChar == 'h')
                {
                    Console.Clear();
                    await CreateNewOrderAndUpdateCustomerOrderTotal();
                }
                else if (result.KeyChar == 'i')
                {
                    Console.Clear();
                    await DeleteOrder();
                }
                else if (result.KeyChar == 'j')
                {
                    Console.Clear();
                    await GetTop10Customers();
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }

        }

        public static async Task QueryCustomer() 
        {
            Database database = _client.GetDatabase("database-v2");
            Container container = database.GetContainer("customer");

            string customerId = "FFD0DD37-1F0E-4E2E-8FAC-EAF45B0E9447";

            //Get a customer with a query
            string sql = $"SELECT * FROM c WHERE c.id = @id";

            FeedIterator<CustomerV2> resultSet = container.GetItemQueryIterator<CustomerV2>(
                new QueryDefinition(sql)
                .WithParameter("@id", customerId),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(customerId)
                });

            Console.WriteLine("Query for a single customer\n");
            while (resultSet.HasMoreResults)
            {
                FeedResponse<CustomerV2> response = await resultSet.ReadNextAsync();

                foreach (CustomerV2 customer in response)
                {
                    Print(customer);
                }

                Console.WriteLine($"Customer Query Request Charge {response.RequestCharge}\n");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        public static async Task GetCustomer()
        {
            Database database = _client.GetDatabase("database-v2");
            Container container = database.GetContainer("customer");

            string customerId = "FFD0DD37-1F0E-4E2E-8FAC-EAF45B0E9447";

            Console.WriteLine("Point Read for a single customer\n");

            //Get a customer with a point read
            ItemResponse<CustomerV2> response =  await container.ReadItemAsync<CustomerV2>(
                id: customerId, 
                partitionKey: new PartitionKey(customerId));

            Print(response.Resource);

            Console.WriteLine($"Point Read Request Charge {response.RequestCharge}\n");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task ListAllProductCategories()
        {
            Database database = _client.GetDatabase("database-v2");
            Container container = database.GetContainer("productCategory");

            //Get all product categories
            string sql = $"SELECT * FROM c WHERE c.type = 'category'";

            FeedIterator<ProductCategory> resultSet = container.GetItemQueryIterator<ProductCategory>(
                new QueryDefinition(sql),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey("category")
                });

            while (resultSet.HasMoreResults)
            {
                FeedResponse<ProductCategory> response = await resultSet.ReadNextAsync();

                Console.WriteLine("Print out product categories\n");
                foreach(ProductCategory productCategory in response)
                {
                    Print(productCategory);
                }
                Console.WriteLine($"Product Category Query Request Charge {response.RequestCharge}\n");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task QueryProductsByCategoryId()
        {
            Database database = _client.GetDatabase("database-v3");
            Container container = database.GetContainer("product");

            //Category Name = Components, Headsets
            string categoryId = "AB952F9F-5ABA-4251-BC2D-AFF8DF412A4A";

            //Query for products by category id
            string sql = $"SELECT * FROM c WHERE c.categoryId = @categoryId";

            FeedIterator<Product> resultSet = container.GetItemQueryIterator<Product>(
                new QueryDefinition(sql)
                .WithParameter("@categoryId", categoryId),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(categoryId)
                });

            while (resultSet.HasMoreResults)
            {
                FeedResponse<Product> response = await resultSet.ReadNextAsync();

                Console.WriteLine("Print out products for the passed in category id\n");
                foreach (Product product in response)
                {
                    Print(product);
                }
                Console.WriteLine($"Product Query Request Charge {response.RequestCharge}\n");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task QueryProductsForCategory()
        {
            Database database = _client.GetDatabase("database-v3");
            Container container = database.GetContainer("product");

            //Category Name = Accessories, Tires and Tubes
            string categoryId = "86F3CBAB-97A7-4D01-BABB-ADEFFFAED6B4";
            
            //Query for this category. How many products?
            string sql = "SELECT COUNT(1) AS ProductCount, c.categoryName FROM c WHERE c.categoryId = '86F3CBAB-97A7-4D01-BABB-ADEFFFAED6B4' GROUP BY c.categoryName";

            FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(
                new QueryDefinition(sql), requestOptions: new QueryRequestOptions{ PartitionKey = new PartitionKey(categoryId)});

            Console.WriteLine("Print out category name and number of products in that category\n");
            while (resultSet.HasMoreResults)
            {
                FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
                foreach (var item in response)
                {
                    Console.WriteLine($"Product Count: {item.ProductCount}\nCategory: {item.categoryName}\n");
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task UpdateProductCategory()
        {
            Database database = _client.GetDatabase("database-v3");
            Container container = database.GetContainer("productCategory");

            string categoryId = "86F3CBAB-97A7-4D01-BABB-ADEFFFAED6B4";
            //Category Name = Accessories, Tires and Tubes

            Console.WriteLine("Update the name and replace 'and' with '&'");
            ProductCategory updatedProductCategory = new ProductCategory
            {
                id = categoryId,
                type = "category",
                name = "Accessories, Tires & Tubes"
            };

            await container.ReplaceItemAsync(
                partitionKey: new PartitionKey("category"),
                id: categoryId,
                item: updatedProductCategory);

            Console.WriteLine("Category updated.\nPress any key to continue...");
            Console.ReadKey();
        }

        public static async Task RevertProductCategory()
        {
            Database database = _client.GetDatabase("database-v3");
            Container container = database.GetContainer("productCategory");

            string categoryId = "86F3CBAB-97A7-4D01-BABB-ADEFFFAED6B4";
            ProductCategory updatedProductCategory = new ProductCategory
            {
                id = categoryId,
                type = "category",
                name = "Accessories, Tires and Tubes"
            };
            Console.WriteLine("Change category name back to original");

            await container.ReplaceItemAsync(
                partitionKey: new PartitionKey("category"),
                id: categoryId,
                item: updatedProductCategory);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task QuerySalesOrdersByCustomerId()
        {
            Database database = _client.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            string customerId = "FFD0DD37-1F0E-4E2E-8FAC-EAF45B0E9447";
            
            string sql = "SELECT * from c WHERE c.type = 'salesOrder' and c.customerId = @customerId";

            FeedIterator<SalesOrder> resultSet = container.GetItemQueryIterator<SalesOrder>(
                new QueryDefinition(sql)
                .WithParameter("@customerId", customerId),
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(customerId) });

            Console.WriteLine("Print out orders for this customer\n");
            while (resultSet.HasMoreResults)
            {
                FeedResponse<SalesOrder> response = await resultSet.ReadNextAsync();
                foreach (SalesOrder salesOrder in response)
                {
                    Print(salesOrder);
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }

        public static async Task QueryCustomerAndSalesOrdersByCustomerId()
        {
            Database database = _client.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            string customerId = "FFD0DD37-1F0E-4E2E-8FAC-EAF45B0E9447";

            string sql = "SELECT * from c WHERE c.customerId = @customerId";

            FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(
                new QueryDefinition(sql)
                .WithParameter("@customerId", customerId),
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(customerId) });

            CustomerV4 customer = new CustomerV4();
            List<SalesOrder> orders = new List<SalesOrder>();

            while (resultSet.HasMoreResults)
            {
                FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
                foreach (var item in response)
                {
                    if (item.type == "customer")
                    {
                        customer = JsonConvert.DeserializeObject<CustomerV4>(item.ToString());
                        
                    }
                    else if (item.type == "salesOrder")
                    {
                        orders.Add(JsonConvert.DeserializeObject<SalesOrder>(item.ToString()));
                    }
                }
            }

            Console.WriteLine("Print out customer record and all their orders\n");
            Print(customer);
            foreach(SalesOrder order in orders)
            {
                Print(order);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task CreateNewOrderAndUpdateCustomerOrderTotal()
        {
            Database database = _client.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            //Get the customer
            string customerId = "FFD0DD37-1F0E-4E2E-8FAC-EAF45B0E9447";
            ItemResponse<CustomerV4> response = await container.ReadItemAsync<CustomerV4>(
                id: customerId, 
                partitionKey: new PartitionKey(customerId)
                );
            CustomerV4 customer = response.Resource;

            //Increment the salesOrderTotal property
            customer.salesOrderCount++;

            //Create a new order
            string orderId = "5350ce31-ea50-4df9-9a48-faff97675ac5"; //Normally would use Guid.NewGuid().ToString()

            SalesOrder salesOrder = new SalesOrder
            {
                id = orderId,
                type = "salesOrder",
                customerId = customer.id,
                orderDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                shipDate = "",
                details = new List<SalesOrderDetails>
                {
                    new SalesOrderDetails
                    {
                        sku = "FR-M94B-38",
                        name = "HL Mountain Frame - Black, 38",
                        price = 1349.6,
                        quantity = 1
                    },
                    new SalesOrderDetails
                    {
                        sku = "SO-R809-M",
                        name = "Racing Socks, M",
                        price = 8.99,
                        quantity = 2
                    }
                }
            };

            //Submit both as a transactional batch
            TransactionalBatchResponse txBatchResponse = await container.CreateTransactionalBatch(
                new PartitionKey(salesOrder.customerId))
                .CreateItem<SalesOrder>(salesOrder)
                .ReplaceItem<CustomerV4>(customer.id, customer)
                .ExecuteAsync();

            if (txBatchResponse.IsSuccessStatusCode)
                Console.WriteLine("Order created successfully");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task DeleteOrder()
        {
            Database database = _client.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            string customerId = "FFD0DD37-1F0E-4E2E-8FAC-EAF45B0E9447";
            string orderId = "5350ce31-ea50-4df9-9a48-faff97675ac5";

            ItemResponse<CustomerV4> response = await container.ReadItemAsync<CustomerV4>(
                id: customerId, 
                partitionKey: new PartitionKey(customerId)
            );
            CustomerV4 customer = response.Resource;

            //Decrement the salesOrderTotal property
            customer.salesOrderCount--;

            //Submit both as a transactional batch
            TransactionalBatchResponse txBatchResponse = await container.CreateTransactionalBatch(
                new PartitionKey(customerId))
                .DeleteItem(orderId)
                .ReplaceItem<CustomerV4>(customer.id, customer)
                .ExecuteAsync();

            if (txBatchResponse.IsSuccessStatusCode)
                Console.WriteLine("Order deleted successfully");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task GetTop10Customers()
        {
            Database database = _client.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            //Query to get our top 10 customers 
            string sql = "SELECT TOP 10 c.firstName, c.lastName, c.salesOrderCount FROM c WHERE c.type = 'customer' ORDER BY c.salesOrderCount DESC";

            FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(new QueryDefinition(sql));

            Console.WriteLine("Print out top 10 customers and number of orders\n");
            double ru = 0;
            while (resultSet.HasMoreResults)
            {
                FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
                foreach (var item in response)
                {
                    Console.WriteLine($"Customer Name: {item.firstName} {item.lastName} \tOrders: {item.salesOrderCount}");
                }
                ru += response.RequestCharge;
            }
            Console.WriteLine($"\nRequest Charge: {ru}\n");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static void Print(object obj)
        {
            Console.WriteLine($"{JObject.FromObject(obj).ToString()}\n");
        }
    }


}
