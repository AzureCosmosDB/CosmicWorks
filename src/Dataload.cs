using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CosmicWorks
{
    class Dataload
    {
        private static string gitdatapath = "https://api.github.com/repos/AzureCosmosDB/CosmicWorks/contents/data/";

        public static async Task LoadData(CosmosClient cosmosDBClient, bool force = false, int? schemaVersion = null)
        {
            await LoadContainersFromGitHub(cosmosDBClient, "database-v1");
            await LoadContainersFromGitHub(cosmosDBClient, "database-v2");
            await LoadContainersFromGitHub(cosmosDBClient, "database-v3");
            await LoadContainersFromGitHub(cosmosDBClient, "database-v4");
            await LoadContainersFromGitHub(cosmosDBClient, "database-v5");
        }

        private static async Task LoadContainersFromGitHub(CosmosClient client, string databaseName)
        {
            Console.WriteLine($"Preparing to load containers for {databaseName} directly from GitHub");

            Database database = client.GetDatabase(databaseName);
            string url = gitdatapath + databaseName;

            using (HttpClient httpClient = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "cosmicworks-samples-cosmosClient");

                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error reading sample data from GitHub");
                    Console.WriteLine($" - {url}");
                    return;
                }

                string directoryJson = await response.Content.ReadAsStringAsync();
                GitFileInfo[] dirContents = JsonConvert.DeserializeObject<GitFileInfo[]>(directoryJson);

                foreach (GitFileInfo file in dirContents)
                {
                    if (file.type == "file")
                    {
                        string containerName = file.name;
                        try
                        {
                            Container container = database.GetContainer(containerName);
                            await LoadContainerFromGitHubFile(container, file.download_url);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error connecting to container {containerName}");
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }

        private static async Task LoadContainerFromGitHubFile(Container container, string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "cosmicworks-samples-cosmosClient");
                HttpResponseMessage response = await client.GetAsync(fileUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error downloading file from {fileUrl}");
                    return;
                }
                string itemsJson = await response.Content.ReadAsStringAsync();
                dynamic itemsArray = JsonConvert.DeserializeObject(itemsJson);
                List<Task> tasks = new List<Task>();
                foreach (var item in itemsArray)
                {
                    tasks.Add(CreateItemWithRetryAsync(container, item));
                }
                await Task.WhenAll(tasks);
            }
        }

        private static async Task CreateItemWithRetryAsync(Container container, dynamic record)
        {
            bool retry = true;
            while (retry)
            {
                try
                {
                    await container.CreateItemAsync(record);
                    break;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    int waitTime = ex.RetryAfter.HasValue ? ex.RetryAfter.Value.Milliseconds : 1000;
                    Console.WriteLine($"Rate limited. Waiting for {waitTime}ms before retrying...");
                    await Task.Delay(waitTime);
                }
            }
        }

        class GitFileInfo
        {
            public string name = "";
            public string type = "";
            public long size = 0;
            public string download_url = "";
        }
    }
}
