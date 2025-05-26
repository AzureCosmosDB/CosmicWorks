# Modeling Demos with Azure Cosmos DB

This folder contains the main application that demonstrates the evolution of data models from v1 to v4 in Azure Cosmos DB.

## Azure Developer CLI (AZD) Integration

When you deploy this application using AZD, an `appsettings.development.json` file will be automatically created in this folder with the following structure:

```json
{
  "uri": "https://your-cosmos-account.documents.azure.com:443/"
}
```

This file is used by the application to connect to your Azure Cosmos DB instance.

## Running the Application

After deploying with AZD:

1. Open the Cosmic Works solution file in Visual Studio
2. Right click the 'modeling-demos' project and set as startup project
3. Press F5 to start the application
4. From the main menu, choose the options to explore the different data model versions

## Authentication

This application uses Azure Managed Identity for authentication to Cosmos DB. The AZD deployment automatically sets up:

1. A user-assigned managed identity
2. Role-based access control (RBAC) permissions on the Cosmos DB instance
3. Permissions for the current user to access the database

No connection keys are needed as authentication happens through Azure AD credentials.