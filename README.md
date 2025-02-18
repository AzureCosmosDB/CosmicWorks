# Cosmic Works

How to migrate a relational data model to Azure Cosmos DB, a distributed, horizontally scalable, NoSQL database.

This sample demonstrates how to migrate a relational database to a distributed, NoSQL database like Azure Cosmos DB.
This repo contains a Powerpoint presentation and a .NET Project that represents the demos for this presentation. You can watch
this presentation from [Igloo Conf 2022](https://youtu.be/TvQEG52eVrI?si=rbXrAV_SwwtbCIX_&t=49)

The main components of this sample include:

* **Program.cs**: A console application. The menu items coincide with demos for the presentation that highlight the evolution of the data models from v1 to v4.

* **ChangeFeed.cs**: The class implements Cosmos DB's change feed processor to monitor the product categories container for changes and then propagates those to the products container. This highlights how to maintain referential integrity between entities.

* **Models.cs**: This project contains all of the data models (and versions of them) for the entities.

* **CosmosManagement.cs**: This class contains the Cosmos DB management SDK classes used to delete and recreate the databases and containers.

## Steps to setup

This sample uploads a large amount of data. If you have slow upload speeds such as a residential cable modem, it is recommended to open this repository in GitHub Codespaces and deploy the Azure services or git clone to a VM in the same region you deploy to. Below are the two methods for setup.

### Run Locally

1. Fork then git clone this repository to your local machine.
1. Open a terminal, type azd auth login. 
1. Type `azd up` and deploy the Cosmos resources. (This is a serverless account so there is no cost to create these resources.)
1. When the deployment is complete, open the Cosmic Works solution file.
1. On the main menu, press 'K' to load the data. 
    1. This can take **up to 60 minutes** when run locally over low bandwidth connections and may time-out requiring you to run `azd down`, then `azd up` again and start over.
    1. You will see retries as it will ingest data faster than Serverless accounts allow for.
    1. Best performance is running in a GitHub Codespace or on a VM in the same region the Cosmos account was provisioned in.
1. After the data is loaded, return to Visual Studio, put any breakpoints for any of the functions you want to step through.
1. Press F5 to start a debug session.


### Run in CodeSpaces

1. Create a new Codespace for this repository.
1. Open a terminal, type azd auth login. 
1. Type `azd up` and deploy the Cosmos resources. (This is a serverless account so there is no cost to create these resources.)
1. When the deployment is complete, navigate to workspaces/CosmicWorks/src folder by typing `cd src`.
1. Then type `dotnet run` in the terminal to start the application.
    1. You may want to make the terminal larger on your screen by dragging it up or clicking the up carrot to maximize the terminal panel.
1. On the main menu, press 'k' to load the data. 
    1. This takes about **7 minutes on a 2 core CodeSpace** to run and you will see retries as it will ingest data faster than Serverless accounts allow for.
    1. After the data is loaded, if you want to run this solution locally, follow the steps to Run Locally. In Visual Studio, open secrets.json and copy and paste the values from the env file in .azure folder into secrets.json so the app can connect to the deployed Cosmos DB account.
1. After the data is loaded, open Program.cs and put any breakpoints for any of the functions you want to step through.
1. Press F5 to start a debug session.

> [!IMPORTANT]
> This solution doesn't charge for RU when not in use but there are data storage costs. If you are not going to run this sample for some time it is recommended to return to the main menu of the application and select option 'L' to delete all the databases. When you return you can recreate these with option 'M', then reload the data with option 'K'
