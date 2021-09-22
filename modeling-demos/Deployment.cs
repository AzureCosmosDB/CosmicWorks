using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;


namespace modeling_demos
{
    class Deployment
    {
    
        // private static readonly string gitdatapath = config["gitdatapath"];
        private static string gitdatapath = "https://api.github.com/repos/MicrosoftDocs/mslearn-cosmosdb-modules-central/contents/data/fullset/";


        public static async Task LoadDatabase(CosmosClient cosmosDBClient, bool force=false, int? schemaVersion=null)
        {
            {
                int schemaVersionStart = 1;
                int schemaVersionEnd = 4;
                if (!(schemaVersion is null))
                {
                    schemaVersionStart = (int)schemaVersion;
                    schemaVersionEnd = (int)schemaVersion;
                }
                else
                {
                    schemaVersionStart = 1;
                    schemaVersionEnd = 4;
                }
                for (int schemaVersionCounter = schemaVersionStart; schemaVersionCounter <= schemaVersionEnd; schemaVersionCounter++)
                {
                    Console.WriteLine($"download started for schema {schemaVersionCounter}");
                    if (force == true)
                    {
                        await GetFilesFromRepo($"database-v{schemaVersionCounter.ToString()}", force = true);
                    }
                    else
                    {
                        await GetFilesFromRepo($"database-v{schemaVersionCounter.ToString()}");
                    }

                    LoadContainersFromFolder(cosmosDBClient, $"database-v{schemaVersionCounter.ToString()}", $"database-v{schemaVersionCounter.ToString()}");
                }
            }
        }

        public static async Task CreateDatabase(CosmosClient cosmosDBClient, bool force=false, int? schemaVersion=null)
        {
            {
                int schemaVersionStart = 1;
                int schemaVersionEnd = 4;
                if (!(schemaVersion is null))
                {
                    schemaVersionStart = (int)schemaVersion;
                    schemaVersionEnd = (int)schemaVersion;
                }
                else
                {
                    schemaVersionStart = 1;
                    schemaVersionEnd = 4;
                }
                for (int schemaVersionCounter = schemaVersionStart; schemaVersionCounter <= schemaVersionEnd; schemaVersionCounter++)
                {
                    Console.WriteLine($"create started for schema {schemaVersionCounter}");
                    await CreateDatabaseAndContainers(cosmosDBClient,$"database-v{schemaVersionCounter.ToString()}", schemaVersionCounter);
                }
            }
        }

        public static async Task DeleteDatabase(CosmosClient cosmosDBClient, bool force = false, int? schemaVersion = null)
        {
            {
                int schemaVersionStart = 1;
                int schemaVersionEnd = 4;
                if (!(schemaVersion is null))
                {
                    schemaVersionStart = (int)schemaVersion;
                    schemaVersionEnd = (int)schemaVersion;
                }
                else
                {
                    schemaVersionStart = 1;
                    schemaVersionEnd = 4;
                }
                for (int schemaVersionCounter = schemaVersionStart; schemaVersionCounter <= schemaVersionEnd; schemaVersionCounter++)
                {
                    Console.WriteLine($"delete started for schema {schemaVersionCounter}");
                    await DeleteDatabaseAndContainers(cosmosDBClient, $"database-v{schemaVersionCounter.ToString()}");
                }
            }
        }
        public static async Task DeleteDatabaseAndContainers(CosmosClient cosmosDBClient, string database)
        {
            Console.WriteLine($"Deteting database and containers");
            Console.WriteLine($"DatabaseName:{database} key:provided");
            Database cosmosDatabase = cosmosDBClient.GetDatabase(database);
            Console.WriteLine($"Are you sure you want to delete {cosmosDatabase.Id} (Y/N) : ");
            string? response = Console.ReadLine();
            if((response??"") == "Y" | (response??"") == "y")
            {
                await cosmosDatabase.DeleteAsync();
                Console.Write("   Database deleted!");
            }
        }

        public static async Task CreateDatabaseAndContainers(CosmosClient cosmosDBClient, string database, int schema)
        {

            Console.WriteLine($"creating database and containers for schema v{schema}");
            Console.WriteLine($"DatabaseName:{database} key:provided");

            List<SchemaDetails>[] DatabaseSchema = new List<SchemaDetails>[5];

            List<SchemaDetails> DatabaseSchema_1 = new List<SchemaDetails> {
                new SchemaDetails {ContainerName="customerAddress",Pk="/id"},
                new SchemaDetails {ContainerName="customerPassword",Pk="/id"},
                new SchemaDetails {ContainerName="product",Pk="/id"},
                new SchemaDetails {ContainerName="productCategory",Pk="/id"},
                new SchemaDetails {ContainerName="productTag",Pk="/id"},
                new SchemaDetails {ContainerName="productTags",Pk="/id"},
                new SchemaDetails {ContainerName="salesOrder",Pk="/id"},
                new SchemaDetails {ContainerName="salesOrderDetail",Pk="/id"}
              };

            List<SchemaDetails> DatabaseSchema_2 = new List<SchemaDetails> {
                new SchemaDetails {ContainerName="customer",Pk="/id"},
                new SchemaDetails {ContainerName="product",Pk="/categoryId"},
                new SchemaDetails {ContainerName="productCategory",Pk="/type"},
                new SchemaDetails {ContainerName="productTag",Pk="/type"},
                new SchemaDetails {ContainerName="salesOrder",Pk="/customerId"}
                };

            List<SchemaDetails> DatabaseSchema_3 = new List<SchemaDetails> {
                new SchemaDetails {ContainerName="customer",Pk= "/id"},
                new SchemaDetails {ContainerName="product",Pk="/categoryId"},
                new SchemaDetails {ContainerName="productCategory",Pk="/type"},
                new SchemaDetails {ContainerName="productTag",Pk= "/type"},
                new SchemaDetails {ContainerName="salesOrder",Pk= "/customerId"}
                };

            List<SchemaDetails> DatabaseSchema_4 = new List<SchemaDetails> {
                 new SchemaDetails {ContainerName= "customer",Pk="/customerId"},
                new SchemaDetails {ContainerName= "product",Pk= "/categoryId"},
                new SchemaDetails {ContainerName= "productMeta",Pk="/type"},
                new SchemaDetails {ContainerName= "salesByCategory",Pk="/categoryId"}
                };

            DatabaseSchema[1] = DatabaseSchema_1;
            DatabaseSchema[2] = DatabaseSchema_2;
            DatabaseSchema[3] = DatabaseSchema_3;
            DatabaseSchema[4] = DatabaseSchema_4;


            if (schema >= 1 & schema <= 4)
            {
                ThroughputProperties throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(4000);
                Database cosmosDatabase = await cosmosDBClient.CreateDatabaseAsync(database, throughputProperties);
                Console.WriteLine($"  {cosmosDatabase.Id} created");
                foreach (var ContainerSchema in DatabaseSchema[schema])
                {
                    Container container = await cosmosDatabase.CreateContainerAsync(ContainerSchema.ContainerName, ContainerSchema.Pk);
                    Console.WriteLine($"  container:{cosmosDatabase.Id}.{container.Id} created!");
                }
            }
        }

        private static async Task GetFilesFromRepo(string databaseName, bool force = false)
        {
            string folder = "data" + Path.DirectorySeparatorChar + databaseName;
            string url = gitdatapath + databaseName;
            Console.WriteLine("Geting file info from repo");
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "cosmicworks-samples-client");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error reading sample data from GitHub");
                Console.WriteLine($" - {url}");
                return;
            }

            String directoryJson = await response.Content.ReadAsStringAsync(); ;

            GitFileInfo[] dirContents = JsonConvert.DeserializeObject<GitFileInfo[]>(directoryJson);
            var downloadTasks = new List<Task>();

            foreach (GitFileInfo file in dirContents)
            {
                if (file.type == "file")
                {
                    Console.WriteLine($"File {file.name} {file.size}");
                    var filePath = folder + Path.DirectorySeparatorChar + file.name;


                    Boolean downloadFile = true;
                    if (File.Exists(filePath))
                    {
                        if (new System.IO.FileInfo(filePath).Length == file.size)
                        {
                            Console.WriteLine("    File exists and matches size");
                            downloadFile = false;
                            if (force == true) downloadFile = true;
                        }
                    }

                    if (downloadFile)
                    {
                        Console.WriteLine($"   Download path {file.download_url}");
                        Console.WriteLine("    Started download...");
                        downloadTasks.Add(HttpGetFile(file.download_url, filePath));
                    }
                }
            }

            Task downloadTask = Task.WhenAll(downloadTasks);
            try
            {
                downloadTask.Wait();
            }
            catch (AggregateException ex)
            {

            }

            if (downloadTask.Status == TaskStatus.Faulted)
            {
                Console.WriteLine("Files failed to download");
                foreach (var task in downloadTasks)
                {
                    Console.WriteLine("Task {0}: {1}", task.Id, task.Status);
                    Console.WriteLine(task.Exception.ToString());
                }
            }
            if (downloadTask.Status == TaskStatus.RanToCompletion) Console.WriteLine("Files download sucessfully");
        }

        private static void LoadContainersFromFolder(CosmosClient client, string SourceDatabaseName, string TargetDatabaseName, bool useBulk=true)
        {
            if(useBulk)
            {
                client.ClientOptions.AllowBulkExecution = true;
            }
            string folder = "data" + Path.DirectorySeparatorChar + SourceDatabaseName;
            Database database = client.GetDatabase(TargetDatabaseName);
            Console.WriteLine("Preparing to load containers");
            string[] fileEntries = Directory.GetFiles(folder);
            List<Task> concurrentLoads = new List<Task>();
            foreach (string fileName in fileEntries)
            {
                var containerName = fileName.Split(Path.DirectorySeparatorChar)[2];
                Console.WriteLine($"    Container {containerName} from {fileName}");
                try
                {
                    Container container = database.GetContainer(containerName);
                    concurrentLoads.Add(LoadContainerFromFile(container, fileName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error connecting to container {containerName} ");
                    Console.WriteLine(ex.ToString());
                }
            }
            Task concurrentLoad = Task.WhenAll(concurrentLoads);
            try
            {
                concurrentLoad.Wait();
            }
            catch (AggregateException ex)
            {

            }

            if (concurrentLoad.Status == TaskStatus.Faulted)
            {
                Console.WriteLine("Sample data load failed");
            }

            foreach (var task in concurrentLoads)
            {
                Console.WriteLine("Task {0}: {1}", task.Id, task.Status);
                if (task.Status == TaskStatus.Faulted)
                {
                    Console.WriteLine($"Task {task.Id} {task.Exception}");

                }
            }

        }

        private static async Task LoadContainerFromFile(Container container, string file, Boolean noBulk = false)
        {
            using (StreamReader streamReader = new StreamReader(file))
            {

                int maxConcurrentTasks = 200;
                bool usebulk = !noBulk;

                string recordsJson = streamReader.ReadToEnd();
                dynamic recordsArray = JsonConvert.DeserializeObject(recordsJson);

                int batches = 0;
                int batchCounter = 0;
                int docCounter = 0;
                List<Task> concurrentTasks = new List<Task>(maxConcurrentTasks);
                int totalDocs = recordsArray.Count;
                foreach (var record in recordsArray)
                {
                    if (usebulk)
                    {
                        concurrentTasks.Add(container.CreateItemAsync(record));
                    }
                    else
                    {
                        container.CreateItemAsync(record);
                    }
                    batchCounter++;
                    if (batchCounter >= maxConcurrentTasks)
                    {
                        docCounter = docCounter + batchCounter;
                        batchCounter = 0;
                        await Task.WhenAll(concurrentTasks);
                        Console.WriteLine($"    loading {file} - batch:{batches} - documents:{docCounter} of {totalDocs}");

                        concurrentTasks.Clear();
                        batches++;
                    }

                }
                Console.WriteLine($"    loaded  {file} - batch:{batches} - documents:{docCounter} of {totalDocs}");
                await Task.WhenAll(concurrentTasks);
            }
        }

        private static async Task HttpGetFile(string url, string filename)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(filename, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }
        }

        class GitFileInfo
        {
            public String name="";
            public String type="";
            public long size=0;
            public String download_url="";
        }

        class Secrets
        {
            public string uri="";
            public string key="";
        };

        public class SchemaDetails
        {
            public string ContainerName;
            public string Pk;
        };

    }
}
