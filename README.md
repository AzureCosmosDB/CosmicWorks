# Cosmic Works

[![Open in VS Code Dev Containers](https://img.shields.io/badge/Open%20in-VS%20Code%20Dev%20Containers-blue?logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode%3A%2F%2Fms-vscode-remote.remote-containers%2FcloneInVolume%3Furl%3Dhttps%3A%2F%2Fgithub.com%2FAzureCosmosDB%2FCosmicWorks)
[![Deploy with AZD](https://img.shields.io/badge/Deploy%20with-Azure%20Developer%20CLI-blue)](https://github.com/AzureCosmosDB/CosmicWorks#deploy-to-azure-azd)

This repository provides a set of demos that demonstrates how to migrate a relational database for a simple ecommerce application to a distributed, NoSQL database like Azure Cosmos DB, designed to scale out as the number of users grow.

This repo contains a [Powerpoint presentation](CosmosDbDataModelingDeck.pptx) that walks through the data modeling concepts demonstrated by the sample application. You can watch the presentation from [Igloo Conf 2022](https://youtu.be/TvQEG52eVrI?si=rbXrAV_SwwtbCIX_&t=49) to get an idea of how these concepts apply to modeling data for this type of database.

The main components of this sample include:

* **Program.cs**: A console application. The menu items coincide with demos for the presentation that highlight the evolution of the data models from a normalized relational data model (v1) to a fully denormalized NoSQL data model (v4).

* **ChangeFeed.cs**: The class implements Cosmos DB's change feed processor to monitor the product categories container for changes and then propagates those to the products container. This highlights how to maintain referential integrity between entities.

* **Models.cs**: This project contains all of the data models (and versions) for the entities.

* **CosmosManagement.cs**: This class contains the Cosmos DB management .NET SDK classes used to delete and recreate the databases and containers. This class doesn't provide any data modeling concepts but is provided to allow users to delete the Cosmos resources when not in use and recreate them when needed.

## Quick Start Options

Choose one of these options to get started:

1. **[Run in VS Code Dev Containers](#run-in-dev-container)** - Consistent dev environment using containers
2. **[Run locally](#run-locally)** - Traditional local development experience

No matter which method you choose, you will need to **[Deploy to Azure (AZD)](#deploy-to-azure-azd)** to create the Azure Cosmos DB account and resources, as well as create an `appsettings.development.json` file necessary to run this sample.

### Run in Dev Container

**Prerequisites**

1. Install [Visual Studio Code](https://code.visualstudio.com/)
1. Install the [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) extension
1. Install Docker:
   - Windows/macOS: [Docker Desktop](https://www.docker.com/products/docker-desktop/)
   - Linux: Docker Engine + Docker CLI (ensure your user can run `docker`)

**Steps**

1. Click the **Open in VS Code Dev Containers** badge at the top of this README
1. Wait for the dev container to build and start (this may take a few minutes the first time)
1. Deploy to Azure using [Deploy to Azure (AZD)](#deploy-to-azure-azd)
1. Once deployed, open the integrated terminal and run:

   ```bash
   cd src
   dotnet run
   ```

1. On the main menu, press 'k' to load data
1. Explore the different functions by pressing the corresponding menu keys
1. Open the [Cosmos DB Data Modeling Deck](CosmosDbDataModelingDeck.pptx)
1. Follow the slides to learn the concepts
1. For each of the demo slides, refer to the speaker notes

## Run Locally

**Prerequisites**

1. [Visual Studio Code](https://code.visualstudio.com/) (or another IDE/editor)
1. [.NET 10 SDK (LTS)](https://dotnet.microsoft.com/download/dotnet/10.0)
1. [Azure Developer CLI (AZD)](https://learn.microsoft.com/azure/developer/azure-developer-cli/overview)
1. An Azure subscription with permission to create resources

**Steps**

1. Clone the repository `git clone https://github.com/AzureCosmosDB/CosmicWorks.git`
1. Deploy to Azure using [Deploy to Azure (AZD)](#deploy-to-azure-azd)
1. Run the application:

   ```bash
   cd src
   dotnet run
   ```

1. On the main menu, press 'k' to load data
1. Open the [Cosmos DB Data Modeling Deck](./CosmosDbDataModelingDeck.pptx)
1. Follow the slides to learn the concepts.
1. For each of the Demo slides, refer to the Speaker notes.
1. Execute the demos by pressing the corresponding menu keys in the app.

## Deploy to Azure (AZD)

This option deploys a serverless Cosmos DB account with all required databases and containers, and sets up RBAC permissions for both a managed identity and the current user.

The deployment will automatically:

1. Create a serverless Cosmos DB account
1. Set up RBAC permissions
1. Create an `appsettings.development.json` file in the *src* folder
1. Configure everything needed to run the application

### Deploy steps

Run these commands once per environment (and any time you need to reprovision):

```bash
az login
azd auth login
azd up
```

### Managing Costs

> [!IMPORTANT]
> This solution doesn't charge for RU when not in use but there are data storage costs of $0.25 USD per container per month for a total of about $5.50 USD. To keep costs minimal, from the main menu of the application, select option 'L' to delete all the databases. When you return, recreate these with option 'M', then reload the data with option 'K'.

## Data Model Progression

The data used in this sample represents 4 versions of our data model as it progresses through the migration from a normalized relational database to a partitioned NoSQL database which scales easily as the number of
users increases. It can be enlightening to see this progression by looking at how the data model changes below through successive versions:

* [Cosmic Works version 1](https://github.com/AzureCosmosDB/CosmicWorks/tree/main/data/database-v1)
* [Cosmic Works version 2](https://github.com/AzureCosmosDB/CosmicWorks/tree/main/data/database-v2)
* [Cosmic Works version 3](https://github.com/AzureCosmosDB/CosmicWorks/tree/main/data/database-v3)
* [Cosmic Works version 4](https://github.com/AzureCosmosDB/CosmicWorks/tree/main/data/database-v4)
