# Change Feed Categories Demo

This project demonstrates the use of the Change Feed processor to monitor the product categories container for changes and propagate those changes to the products container.

## Azure Developer CLI (AZD) Integration

When you deploy this application using AZD, an `appsettings.development.json` file will be automatically created in this folder with the following structure:

```json
{
  "uri": "https://your-cosmos-account.documents.azure.com:443/"
}
```

This file is used by the application to connect to your Azure Cosmos DB instance.

## Running the Application

After deploying with AZD and running the modeling-demos application to populate the databases:

1. Open the Cosmic Works solution file in Visual Studio
2. Right click the 'change-feed-categories' project and select Debug > Start New Instance
3. The application will start monitoring the product categories container for changes

## Authentication

This application uses Azure Managed Identity for authentication to Cosmos DB. The AZD deployment automatically sets up:

1. A user-assigned managed identity
2. Role-based access control (RBAC) permissions on the Cosmos DB instance
3. Permissions for the current user to access the database

No connection keys are needed as authentication happens through Azure AD credentials.