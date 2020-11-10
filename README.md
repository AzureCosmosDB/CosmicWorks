# CosmicWorks

**This is not final so links may not yet work**

Designing a relational data model on Azure Cosmos DB

This repo contains a Visual Studio solution with three projects in it:

* **modeling-demos**: This contains the main app that shows the evolution of the data models from v1 to v4

* **change-feed-categories**: This project uses change feed processor to monitor the product categories container for changes and then propagates those to the products container.

* **models**: This project contains all of the POCO classes used in both other projects.

You can download all of the data for each of the 4 versions of this from these links below.

* [Cosmic Works version 1](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v1?sv=2019-10-10&st=2020-11-09T23%3A16%3A48Z&se=2100-11-10T23%3A16%3A00Z&sr=c&sp=rl&sig=asXotuqtJ62UVr7eJshQAFq1Riw27eCPu8OvnXgmSHg%3D)

* [Cosmic Works version 2](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v2?sv=2019-10-10&st=2020-11-09T23%3A18%3A01Z&se=2100-11-10T23%3A18%3A00Z&sr=c&sp=rl&sig=Z%2FQRziaK3mR5NMDybWOd5yx8ledLQ9NoLUiX7NqmCz8%3D)

* [Cosmic Works version 3](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v3?sv=2019-10-10&st=2020-11-09T23%3A18%3A20Z&se=2100-11-10T23%3A18%3A00Z&sr=c&sp=rl&sig=Be0U3JKnABeU3o8QUsKfTOQdrlymcdBEp2Z6rZMb0Ls%3D)

* [Cosmic Works version 4](https://cosmosdbcosmicworks.blob.core.windows.net/cosmic-works-v4?sv=2019-10-10&st=2020-11-09T23%3A18%3A38Z&se=2100-11-10T23%3A18%3A00Z&sr=c&sp=rl&sig=z6qwIjTFMHSNrtMaDd4uNnRAuvvFSZ%2BHQi5PMSj1pE8%3D)

You can also [download a bak file](https://cosmosdbcosmicworks.blob.core.windows.net/adventure-works-2017?sv=2019-10-10&st=2020-11-09T23%3A15%3A36Z&se=2100-11-10T23%3A15%3A00Z&sr=c&sp=rl&sig=eMlCaQBCuuXpV67IaVyK5sQXti0O06ePhGETcmhHhWA%3D) for the original Adventure Works 2017 database this session and app is built upon.

To create a new Cosmos DB account with four databases and containers for each from this button below. The four databases are set up with autoscale throughput. To improve the performance of the import process you may want to increase the throughput, then reduce it back to 4000 RU/s. Note that the data in blob storage is located in West US 2. If you create the Cosmos DB account in this region you will get better performance and latency when bulk loading the data.

[![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fazurecosmosdb%2Fcosmicworks%2Fmaster%2Fazuredeploy.json)

