# CosmicWorks

How to migrate a relational data model to Azure Cosmos DB, a distributed, horizontally scalable, NoSQL database.

This sample demonstrates how to migrate a relational database to a distributed, NoSQL database like Azure Cosmos DB.
This repo contains a Powerpoint presentation and a Visual Studio solution that represents the demos for this presentation with three projects in it:

* **modeling-demos**: This contains the main app that shows the evolution of the data models from v1 to v4

* **change-feed-categories**: This project uses change feed processor to monitor the product categories container for changes and then propagates those to the products container.

* **models**: This project contains all of the POCO classes used in both projects.

* **cosmos-management**: This project contains the Cosmos DB management SDK classes used to create the databases and containers.

## Steps to setup

### Option 1: Quick Deploy

[![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fazurecosmosdb%2Fcosmicworks%2Fmaster%2Fazuredeploy.json)

1. Clone this repository to your local machine.
1. Click the Deploy to Azure button above. This will provision a new Cosmos DB account in a single region.
1. When the deployment is complete, click on the Outputs tab in the custom deployment blade. Copy the `uri` and `key` values and save locally.
1. Open the Cosmic Works solution file.
1. Add the `uri` and `key` information to the appSettings.json file for both the 'change-feed-categories' and 'modeling-demos' VS Project files or right click each project, select 'Manage User Secrets' and enter the same key and values as key-value pairs there.

### Option 2: Using Azure Developer CLI (AZD)

This option deploys a serverless Cosmos DB account with all required databases and containers, and sets up RBAC permissions for both a managed identity and the current user.

1. Install the [Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
2. Clone this repository to your local machine.
3. Run the following commands from the repository root:

```bash
# Login to Azure
azd auth login

# Initialize the environment (first time only)
azd init

# Provision resources and deploy
azd up
```

The deployment will automatically create an `appsettings.development.json` file in the modeling-demos folder with the Cosmos DB endpoint.
1. Right click the 'modeling-demos' project and set as start up. Then press F5 to start it.
1. On the main menu, press 'k' to create the database and container resources. (This is a serverless account so there is no cost to create these resources.)
1. On the main menu, press 'l' to load data. (Note, this can take some time when run locally over low bandwidth connections. Best performance is running in a GitHub Codespace or on a VM in the same region the Cosmos account was provisioned in.)
1. After the data is loaded, return to Visual Studio, right click the 'change-feed-categories' project and select, Debug, Start a new instance.
1. In the modeling-demos project, put breakpoints for any of the functions you want to step through, then press the corresponding menu item key to execute.

> [!IMPORTANT]
> This runs in a serverless Cosmos DB account so you will not incur RU charges when not running. But you will pay for storage.
If you do not plan to run this sample for a long time, to minimize cost, it is recommended to run the 'Delete databases and containers' item from the main menu. This will delete the databases and containers and just leave an empty Cosmos account which has no cost. 
You can then start the sample again and run 'k' and  'l' menu items to rehydrate the account.

## Source data

Below is the source data for the 4 versions of the Cosmos DB databases as it progresses through its evolution from a relational database to a highly scalable NoSQL database.


* [Cosmic Works version 1](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/database-v1)

* [Cosmic Works version 2](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/database-v2)

* [Cosmic Works version 3](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/database-v3)

* [Cosmic Works version 4](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/database-v4)

You can also [download a bak file](https://github.com/AzureCosmosDB/CosmicWorks/tree/master/data/adventure-works-2017) for the original Adventure Works 2017 database this session and app is built upon.
