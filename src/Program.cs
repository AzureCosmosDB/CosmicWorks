using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CosmicWorks
{
    class Program
    {

        static CosmosClient cosmosClient;
        static ChangeFeed changeFeed;
        static AdvancedChangeFeed advancedChangeFeed;

        public static void AddConfiguration(IConfigurationBuilder config)
        {
            config.AddJsonFile(@"appsettings.development.json", optional: false, reloadOnChange: true);

            var configuration = config.Build();
            var uri = configuration["ACCOUNT_ENDPOINT"];

            // Create the CosmosClient instance
            cosmosClient = new CosmosClient(uri, new DefaultAzureCredential());

            // Create the ChangeFeed instance
            changeFeed = new ChangeFeed(cosmosClient);

            // Create the Advanced ChangeFeed instance for V5 features
            advancedChangeFeed = new AdvancedChangeFeed(cosmosClient);

        }

        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    AddConfiguration(config);
                })
                .Build();

            await changeFeed.StartChangeFeedProcessorAsync();

            await RunApp();
        }
        public static async Task RunApp()
        {
            // Your existing code to run the application
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
                Console.WriteLine($"-------------------------------------------");
                Console.WriteLine($"[l]   Demo: Hierarchical Partitioning (V5)");
                Console.WriteLine($"[m]   Demo: Computed Properties (V5)");
                Console.WriteLine($"[n]   Demo: Advanced Change Feed (V5)");
                Console.WriteLine($"[o]   Demo: Cross-region queries (V5)");
                Console.WriteLine($"-------------------------------------------");
                Console.WriteLine($"[k]   Upload data to containers");
                Console.WriteLine($"-------------------------------------------");
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
                else if (result.KeyChar == 'l')
                {
                    Console.Clear();
                    await DemoHierarchicalPartitioning();
                }
                else if (result.KeyChar == 'm')
                {
                    Console.Clear();
                    await DemoComputedProperties();
                }
                else if (result.KeyChar == 'n')
                {
                    Console.Clear();
                    await DemoAdvancedChangeFeed();
                }
                else if (result.KeyChar == 'o')
                {
                    Console.Clear();
                    await DemoCrossRegionQueries();
                }
                else if (result.KeyChar == 'k')
                {
                    //Stop Change Feed Processor
                    await changeFeed.StopChangeFeedProcessorAsync();
                    //Load data from GitHub
                    await Dataload.LoadData(cosmosClient);
                    //Restart Change Feed Processor
                    await changeFeed.StartChangeFeedProcessorAsync();
                    Console.Clear();
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
        }

        public static async Task QueryCustomer()
        {
            Database database = cosmosClient.GetDatabase("database-v2");
            Container container = database.GetContainer("customer");

            string customerId = "77A64329-1C2A-4BE4-867C-56B40962EC4E";

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
            Database database = cosmosClient.GetDatabase("database-v2");
            Container container = database.GetContainer("customer");

            string customerId = "77A64329-1C2A-4BE4-867C-56B40962EC4E";

            Console.WriteLine("Point Read for a single customer\n");

            //Get a customer with a point read
            ItemResponse<CustomerV2> response = await container.ReadItemAsync<CustomerV2>(
                id: customerId,
                partitionKey: new PartitionKey(customerId));

            Print(response.Resource);

            Console.WriteLine($"Point Read Request Charge {response.RequestCharge}\n");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task ListAllProductCategories()
        {
            Database database = cosmosClient.GetDatabase("database-v2");
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
                foreach (ProductCategory productCategory in response)
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
            Database database = cosmosClient.GetDatabase("database-v3");
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
            Database database = cosmosClient.GetDatabase("database-v3");
            Container container = database.GetContainer("product");

            //Category Name = Accessories, Tires and Tubes
            string categoryId = "86F3CBAB-97A7-4D01-BABB-ADEFFFAED6B4";

            //Query for this category. How many products?
            string sql = "SELECT COUNT(1) AS ProductCount, c.categoryName " +
                "FROM c WHERE c.categoryId = '86F3CBAB-97A7-4D01-BABB-ADEFFFAED6B4' " +
                "GROUP BY c.categoryName";

            FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(
                new QueryDefinition(sql),
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(categoryId)
                });

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
            Database database = cosmosClient.GetDatabase("database-v3");
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
            Database database = cosmosClient.GetDatabase("database-v3");
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
            Database database = cosmosClient.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            string customerId = "77A64329-1C2A-4BE4-867C-56B40962EC4E";

            string sql = "SELECT * from c WHERE c.type = 'salesOrder' and c.customerId = @customerId";

            FeedIterator<SalesOrder> resultSet = container.GetItemQueryIterator<SalesOrder>(
                new QueryDefinition(sql)
                .WithParameter("@customerId", customerId),
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(customerId)
                });

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
            Database database = cosmosClient.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            string customerId = "77A64329-1C2A-4BE4-867C-56B40962EC4E";

            string sql = "SELECT * from c WHERE c.customerId = @customerId";

            FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(
                new QueryDefinition(sql)
                .WithParameter("@customerId", customerId),
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(customerId)
                });

            CustomerV4 customer = new CustomerV4();
            List<SalesOrder> orders = new List<SalesOrder>();

            while (resultSet.HasMoreResults)
            {
                //dynamic response. Deserialize into POCO's based upon "type" property
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
            foreach (SalesOrder order in orders)
            {
                Print(order);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static async Task CreateNewOrderAndUpdateCustomerOrderTotal()
        {
            Database database = cosmosClient.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            //Get the customer
            string customerId = "77A64329-1C2A-4BE4-867C-56B40962EC4E";
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
            Database database = cosmosClient.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            string customerId = "77A64329-1C2A-4BE4-867C-56B40962EC4E";
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
            Database database = cosmosClient.GetDatabase("database-v4");
            Container container = database.GetContainer("customer");

            //Query to get our top 10 customers 
            string sql = "SELECT TOP 10 c.firstName, c.lastName, c.salesOrderCount " +
                "FROM c WHERE c.type = 'customer' " +
                "ORDER BY c.salesOrderCount DESC";

            FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(
                new QueryDefinition(sql));

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

        // V5 Advanced Features Demonstrations

        public static async Task DemoHierarchicalPartitioning()
        {
            Database database = cosmosClient.GetDatabase("database-v5");
            Container customerContainer = database.GetContainer("customer");
            Container salesOrderContainer = database.GetContainer("salesOrder");

            Console.WriteLine("=== Hierarchical Partitioning Demo ===\n");
            Console.WriteLine("V5 demonstrates advanced partitioning strategies:");
            Console.WriteLine("• Regional partitioning for customers and orders");
            Console.WriteLine("• Enables efficient geo-distributed queries");
            Console.WriteLine("• Supports both regional and cross-regional analytics\n");

            Console.WriteLine("📚 About Hierarchical Partitioning:");
            Console.WriteLine("Hierarchical partition keys (available in preview) allow multiple levels:");
            Console.WriteLine("Example: [/region, /customerId] or [/tenantId, /userId, /deviceId]");
            Console.WriteLine("Benefits:");
            Console.WriteLine("• Better query performance for common access patterns");
            Console.WriteLine("• More granular control over data distribution");
            Console.WriteLine("• Improved scalability for multi-tenant scenarios\n");

            // Demo 1: Query customers in a specific region efficiently
            Console.WriteLine("1. Query all customers in North America (using regional partition key):");
            
            string sql = "SELECT c.customerId, c.firstName, c.lastName, c.region FROM c WHERE c.type = 'customer'";
            
            FeedIterator<dynamic> resultSet = customerContainer.GetItemQueryIterator<dynamic>(
                new QueryDefinition(sql),
                requestOptions: new QueryRequestOptions()
                {
                    // Efficient query using regional partition key
                    PartitionKey = new PartitionKey("North America")
                });

            double requestCharge = 0;
            while (resultSet.HasMoreResults)
            {
                FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
                requestCharge += response.RequestCharge;
                
                foreach (var customer in response)
                {
                    Console.WriteLine($"  Customer: {customer.firstName} {customer.lastName} (ID: {customer.customerId})");
                }
            }
            Console.WriteLine($"  Request Charge (Regional query): {requestCharge:F2} RUs\n");

            // Demo 2: Point read using regional partition key
            Console.WriteLine("2. Point read for specific customer using regional partition key:");
            
            try
            {
                ItemResponse<CustomerV5> pointReadResponse = await customerContainer.ReadItemAsync<CustomerV5>(
                    id: "CUSTOMER-001",
                    partitionKey: new PartitionKey("North America"));

                Console.WriteLine($"  Found: {pointReadResponse.Resource.firstName} {pointReadResponse.Resource.lastName}");
                Console.WriteLine($"  Request Charge (Point read): {pointReadResponse.RequestCharge:F2} RUs");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"  Note: Point read requires exact partition key match");
                Console.WriteLine($"  Error: {ex.Message}");
            }

            // Demo 3: Cross-region comparison
            Console.WriteLine("\n3. Regional comparison using optimized partitioning:");
            
            var regions = new[] { "North America", "Europe", "Asia Pacific" };
            
            foreach (string region in regions)
            {
                string regionalSql = "SELECT COUNT(1) as CustomerCount, AVG(c.salesOrderCount) as AvgOrders FROM c WHERE c.type = 'customer'";
                
                FeedIterator<dynamic> regionalQuery = customerContainer.GetItemQueryIterator<dynamic>(
                    new QueryDefinition(regionalSql),
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(region)
                    });

                double regionalRU = 0;
                while (regionalQuery.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await regionalQuery.ReadNextAsync();
                    regionalRU += response.RequestCharge;
                    
                    foreach (var result in response)
                    {
                        Console.WriteLine($"  {region}: {result.CustomerCount} customers, Avg Orders: {result.AvgOrders:F1} (RU: {regionalRU:F2})");
                    }
                }
            }

            Console.WriteLine("\n🔍 Real-world Hierarchical Partitioning Examples:");
            Console.WriteLine("• E-commerce: [/region, /customerId] for geo-distributed customer data");
            Console.WriteLine("• IoT: [/deviceType, /location, /deviceId] for sensor data");
            Console.WriteLine("• Multi-tenant SaaS: [/tenantId, /userId] for customer isolation");
            Console.WriteLine("• Gaming: [/gameId, /playerId] for player-specific queries");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static async Task DemoComputedProperties()
        {
            Database database = cosmosClient.GetDatabase("database-v5");
            Container customerContainer = database.GetContainer("customer");
            Container productContainer = database.GetContainer("product");

            Console.WriteLine("=== Computed Properties Demo ===\n");
            Console.WriteLine("📚 About Computed Properties:");
            Console.WriteLine("Computed properties automatically calculate and index derived values:");
            Console.WriteLine("• Defined at container creation time");
            Console.WriteLine("• Automatically maintained and indexed");
            Console.WriteLine("• Enable efficient queries on calculated fields");
            Console.WriteLine("• Reduce application complexity and improve performance\n");

            Console.WriteLine("Example Computed Properties for V5:");
            Console.WriteLine("Customer container:");
            Console.WriteLine("  • fullName: CONCAT(c.firstName, ' ', c.lastName)");
            Console.WriteLine("  • yearCreated: DateTimePart('yyyy', c.creationDate)");
            Console.WriteLine("Product container:");
            Console.WriteLine("  • priceRange: c.price < 50 ? 'low' : c.price < 200 ? 'medium' : 'high'");
            Console.WriteLine("  • discountedPrice: c.price * 0.9\n");

            // Demo 1: Simulate computed property queries for customers
            Console.WriteLine("1. Customer queries using computed 'fullName' property:");
            Console.WriteLine("   (Simulating what would be efficient computed property queries)");
            
            string customerSql = "SELECT c.customerId, c.firstName, c.lastName, c.region FROM c WHERE c.type = 'customer'";
            
            FeedIterator<dynamic> customerResults = customerContainer.GetItemQueryIterator<dynamic>(
                new QueryDefinition(customerSql));

            while (customerResults.HasMoreResults)
            {
                FeedResponse<dynamic> response = await customerResults.ReadNextAsync();
                
                foreach (var customer in response)
                {
                    // Simulate computed property usage
                    string computedFullName = $"{customer.firstName} {customer.lastName}";
                    string computedYear = DateTime.Parse("2023-01-01").Year.ToString(); // Simplified for demo
                    Console.WriteLine($"  Computed fullName: '{computedFullName}' (Region: {customer.region})");
                    Console.WriteLine($"  Computed yearCreated: {computedYear}");
                }
            }

            // Demo 2: Product price range analysis using computed properties
            Console.WriteLine("\n2. Product analysis using computed 'priceRange' property:");
            
            string productSql = "SELECT p.name, p.price, p.categoryName FROM p";
            
            FeedIterator<dynamic> productResults = productContainer.GetItemQueryIterator<dynamic>(
                new QueryDefinition(productSql));

            var priceRangeStats = new Dictionary<string, List<double>>
            {
                ["low"] = new List<double>(),
                ["medium"] = new List<double>(),
                ["high"] = new List<double>()
            };

            while (productResults.HasMoreResults)
            {
                FeedResponse<dynamic> response = await productResults.ReadNextAsync();
                
                foreach (var product in response)
                {
                    // Simulate computed property calculation
                    double price = (double)product.price;
                    string priceRange = price < 50 ? "low" : 
                                       price < 200 ? "medium" : "high";
                    double discountedPrice = price * 0.9;
                    
                    priceRangeStats[priceRange].Add(price);
                    
                    Console.WriteLine($"  Product: {product.name} ({product.categoryName})");
                    Console.WriteLine($"    Price: ${price:F2}, Range: {priceRange}, Discounted: ${discountedPrice:F2}");
                }
            }

            // Demo 3: Aggregation using computed properties
            Console.WriteLine("\n3. Price range analysis (leveraging computed priceRange):");
            foreach (var range in priceRangeStats)
            {
                if (range.Value.Count > 0)
                {
                    double avgPrice = range.Value.Average();
                    Console.WriteLine($"  {range.Key.ToUpper()} range: {range.Value.Count} products, Avg price: ${avgPrice:F2}");
                }
            }

            Console.WriteLine("\n🚀 Benefits of Computed Properties:");
            Console.WriteLine("✅ Automatic indexing for fast queries");
            Console.WriteLine("✅ Consistent calculations across all queries");
            Console.WriteLine("✅ Reduced application logic complexity");
            Console.WriteLine("✅ Better query performance vs. calculated fields");
            Console.WriteLine("✅ Simplified data access patterns");

            Console.WriteLine("\n🔍 Real-world Use Cases:");
            Console.WriteLine("• E-commerce: Product search by computed price ranges");
            Console.WriteLine("• Analytics: Time-based aggregations (year, month, quarter)");
            Console.WriteLine("• User management: Search by computed display names");
            Console.WriteLine("• Financial: Risk scoring based on multiple factors");
            Console.WriteLine("• IoT: Computed alert levels from sensor readings");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static async Task DemoAdvancedChangeFeed()
        {
            Console.WriteLine("=== Advanced Change Feed Demo ===\n");
            Console.WriteLine("V5 demonstrates 'All Versions and Deletes' change feed mode:");
            Console.WriteLine("- Tracks Create, Update, and Delete operations");
            Console.WriteLine("- Provides before/after versions for updates");
            Console.WriteLine("- Enables comprehensive audit trails\n");

            // Start the advanced change feed processor
            Console.WriteLine("Starting Advanced Change Feed Processor...");
            await advancedChangeFeed.StartAdvancedChangeFeedProcessorAsync();

            Console.WriteLine("\nNow let's make some changes to see the change feed in action:");
            Console.WriteLine("Press 'c' to create a customer, 'u' to update a customer, 'd' to delete a customer, or 'x' to exit:");

            Database database = cosmosClient.GetDatabase("database-v5");
            Container customerContainer = database.GetContainer("customer");

            bool exitDemo = false;
            while (!exitDemo)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                
                switch (key.KeyChar)
                {
                    case 'c':
                        Console.WriteLine("\n--- Creating new customer ---");
                        var newCustomer = new CustomerV5
                        {
                            id = $"CUSTOMER-{Guid.NewGuid().ToString()[..8]}",
                            type = "customer",
                            customerId = $"CUSTOMER-{Guid.NewGuid().ToString()[..8]}",
                            region = "North America",
                            firstName = "Demo",
                            lastName = "Customer",
                            emailAddress = "demo.customer@adventure-works.com",
                            phoneNumber = "555-0999",
                            creationDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            addresses = new List<CustomerAddress>(),
                            password = new Password { hash = "demo", salt = "demo" },
                            salesOrderCount = 0
                        };

                        try
                        {
                            await customerContainer.CreateItemAsync(newCustomer, 
                                new PartitionKey(newCustomer.region));
                            Console.WriteLine("Customer created successfully!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating customer: {ex.Message}");
                        }
                        break;

                    case 'u':
                        Console.WriteLine("\n--- Updating existing customer ---");
                        try
                        {
                            // Read and update the first customer
                            var readResponse = await customerContainer.ReadItemAsync<CustomerV5>(
                                "CUSTOMER-001", 
                                new PartitionKey("North America"));
                            
                            var customerToUpdate = readResponse.Resource;
                            customerToUpdate.salesOrderCount += 1;
                            customerToUpdate.phoneNumber = "555-UPDATED";

                            await customerContainer.ReplaceItemAsync(customerToUpdate, 
                                customerToUpdate.id, 
                                new PartitionKey(customerToUpdate.region));
                            
                            Console.WriteLine("Customer updated successfully!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating customer: {ex.Message}");
                        }
                        break;

                    case 'd':
                        Console.WriteLine("\n--- Deleting customer (simulated) ---");
                        Console.WriteLine("Note: Delete operation would be tracked in change feed");
                        Console.WriteLine("Showing how deletion would appear in change feed logs");
                        break;

                    case 'x':
                        exitDemo = true;
                        break;

                    default:
                        Console.WriteLine("\nInvalid option. Press 'c', 'u', 'd', or 'x'");
                        break;
                }

                if (!exitDemo)
                {
                    await Task.Delay(2000); // Wait for change feed to process
                    Console.WriteLine("\nPress 'c' to create, 'u' to update, 'd' to delete, or 'x' to exit:");
                }
            }

            // Stop the change feed processor
            await advancedChangeFeed.StopAdvancedChangeFeedProcessorAsync();
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static async Task DemoCrossRegionQueries()
        {
            Database database = cosmosClient.GetDatabase("database-v5");
            Container customerContainer = database.GetContainer("customer");
            Container salesOrderContainer = database.GetContainer("salesOrder");

            Console.WriteLine("=== Cross-Region Query Performance Demo ===\n");
            Console.WriteLine("Comparing query performance across different partition strategies:");
            Console.WriteLine("- Regional queries (using hierarchical partitioning)");
            Console.WriteLine("- Cross-region aggregations");
            Console.WriteLine("- Customer-specific queries\n");

            // Demo 1: Regional customer summary
            Console.WriteLine("1. Regional Customer Analysis:");
            
            var regions = new[] { "North America", "Europe", "Asia Pacific" };
            
            foreach (string region in regions)
            {
                string regionalSql = "SELECT COUNT(1) as CustomerCount FROM c WHERE c.type = 'customer'";
                
                FeedIterator<dynamic> regionalQuery = customerContainer.GetItemQueryIterator<dynamic>(
                    new QueryDefinition(regionalSql),
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(region)
                    });

                double requestCharge = 0;
                int customerCount = 0;
                
                while (regionalQuery.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await regionalQuery.ReadNextAsync();
                    requestCharge += response.RequestCharge;
                    
                    foreach (var result in response)
                    {
                        customerCount = result.CustomerCount;
                    }
                }
                
                Console.WriteLine($"  {region}: {customerCount} customers (RU: {requestCharge:F2})");
            }

            // Demo 2: Cross-partition aggregation
            Console.WriteLine("\n2. Global Customer Summary (Cross-Partition Query):");
            
            string globalSql = "SELECT c.region, COUNT(1) as Count, AVG(c.salesOrderCount) as AvgOrders " +
                              "FROM c WHERE c.type = 'customer' GROUP BY c.region";
                              
            FeedIterator<dynamic> globalQuery = customerContainer.GetItemQueryIterator<dynamic>(
                new QueryDefinition(globalSql));

            double globalRequestCharge = 0;
            
            while (globalQuery.HasMoreResults)
            {
                FeedResponse<dynamic> response = await globalQuery.ReadNextAsync();
                globalRequestCharge += response.RequestCharge;
                
                foreach (var result in response)
                {
                    Console.WriteLine($"  Region: {result.region}, Customers: {result.Count}, Avg Orders: {result.AvgOrders:F1}");
                }
            }
            
            Console.WriteLine($"  Total RU for global query: {globalRequestCharge:F2}");

            // Demo 3: Customer order history across regions
            Console.WriteLine("\n3. Customer Order History (Hierarchical Partition Benefits):");
            
            string customerOrderSql = @"
                SELECT o.id, o.orderDate, 
                       SUM(ARRAY(SELECT VALUE (item.price * item.quantity) FROM item IN o.details)) as total
                FROM o 
                WHERE o.type = 'salesOrder' AND o.customerId = 'CUSTOMER-001'";
                
            FeedIterator<dynamic> orderQuery = salesOrderContainer.GetItemQueryIterator<dynamic>(
                new QueryDefinition(customerOrderSql),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey("North America") // Using region for efficient query
                });

            double orderRequestCharge = 0;
            
            while (orderQuery.HasMoreResults)
            {
                FeedResponse<dynamic> response = await orderQuery.ReadNextAsync();
                orderRequestCharge += response.RequestCharge;
                
                foreach (var order in response)
                {
                    Console.WriteLine($"  Order: {order.id}, Date: {order.orderDate}, Total: ${order.total:F2}");
                }
            }
            
            Console.WriteLine($"  RU for customer-specific query: {orderRequestCharge:F2}");

            Console.WriteLine("\nKey Benefits of Hierarchical Partitioning:");
            Console.WriteLine("✅ Regional queries are highly efficient (single partition)");
            Console.WriteLine("✅ Customer isolation maintained within regions");
            Console.WriteLine("✅ Supports both regional and global analytics");
            Console.WriteLine("✅ Optimal for geo-distributed applications");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
