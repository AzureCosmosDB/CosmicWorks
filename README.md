# CosmicWorks

**This is not final so links may not yet work**

Designing a relational data model on Azure Cosmos DB

This repo contains a Visual Studio solution with three projects in it:

* **modeling-demos**: This contains the main app that shows the evolution of the data models from v1 to v4

* **change-feed-categories**: This project uses change feed processor to monitor the product categories container for changes and then propagates those to the products container.

* **models**: This project contains all of the POCO classes used in both other projects.

You can download all of the data for each of the 4 versions of the Cosmos DB databases as it progresses through its evolution by connecting Storage Explorer to the [Cosmos DB Cosmic Works Storage Account](https://cosmosdbcosmicworks.blob.core.windows.net). 
You can see the contents of these storage containers below.

* [Cosmic Works version 1](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v1?restype=container&comp=list)

* [Cosmic Works version 2](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v2?restype=container&comp=list)

* [Cosmic Works version 3](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v3?restype=container&comp=list)

* [Cosmic Works version 4](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v4?restype=container&comp=list)

You can also [download a bak file](https://cosmosdbcosmicworks.blob.core.windows.net/adventure-works-2017?restype=container&comp=list) for the original Adventure Works 2017 database this session and app is built upon.

To create a new Cosmos DB account with four databases and containers for each from this button below. The four databases are set up with autoscale throughput. To improve the performance of the import process you may want to increase the throughput, then reduce it back to 4000 RU/s. Note that the data in blob storage is located in West US 2. If you create the Cosmos DB account in this region you will get better performance and latency when bulk loading the data.

[![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fazurecosmosdb%2Fcosmicworks%2Fmaster%2Fazuredeploy.json)

