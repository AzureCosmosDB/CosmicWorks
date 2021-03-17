# CosmicWorks

How to migrate a relational data model to Azure Cosmos DB, a distributed, horizontally scalable, NoSQL database.

This repo is used to support a presentation on how to migrate a relational database schema to a NoSQL database like Azure Cosmos DB. 
This repo contains a Powerpoint presetnation and a Visual Studio solution that represents the demos for this presenation with three projects in it:

* **modeling-demos**: This contains the main app that shows the evolution of the data models from v1 to v4

* **change-feed-categories**: This project uses change feed processor to monitor the product categories container for changes and then propagates those to the products container.

* **models**: This project contains all of the POCO classes used in both other projects.

## Source data

You can download all of the data for each of the 4 versions of the Cosmos DB databases as it progresses through its evolution from the data folder in this repository.
You can see the contents of these storage containers below.

* [Cosmic Works version 1](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/cosmic-works-v1)

* [Cosmic Works version 2](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/cosmic-works-v2)

* [Cosmic Works version 3](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/cosmic-works-v3)

* [Cosmic Works version 4](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/cosmic-works-v4)

You can also [download a bak file](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/adventure-works-2017) for the original Adventure Works 2017 database this session and app is built upon.

## Provision the four versions of the Cosmos databases

To create a new Cosmos DB account with four databases and containers for each from this button below. The four databases are set up with autoscale throughput. 
To improve the performance of the import process you may want to increase the throughput to approx. 40,000 RU/s, then reduce it back to 4000 RU/s. Note that the data in blob storage is located in West US 2. If you provision your Cosmos account in another region it will slow load times and also incur egress charges however these will be small.

[![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fazurecosmosdb%2Fcosmicworks%2Fmaster%2Fazuredeploy.json)

## Loading data

If you want to load the data for each of these database versions into Cosmos you can use the [Data Migration Tool](https://docs.microsoft.com/en-us/azure/cosmos-db/import-data) or 
[Azure Data Factory](https://docs.microsoft.com/en-us/azure/data-factory/connector-azure-cosmos-db)
