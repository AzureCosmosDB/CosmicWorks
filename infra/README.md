# CosmicWorks Infrastructure

This folder contains the Bicep templates that define all Azure resources required for the CosmicWorks application.

## Resources deployed

- Azure Cosmos DB account (serverless)
- Multiple databases and containers
- User-assigned managed identity
- RBAC assignments

## Deployment methods

### Using Azure Developer CLI (AZD)

1. Install the [Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
2. Run the following commands:

```bash
# Login to Azure
azd auth login

# Initialize the environment (first time only)
azd init

# Provision resources and deploy
azd up
```

This will:
1. Create all required Azure resources
2. Set up RBAC permissions for both the managed identity and your current user
3. Generate an `appsettings.development.json` file with the Cosmos DB endpoint

### Using Bicep directly

```bash
# Login to Azure
az login

# Create a resource group
az group create --name cosmicworks-rg --location eastus

# Deploy the Bicep template
az deployment group create \
  --resource-group cosmicworks-rg \
  --template-file main.bicep \
  --parameters main.parameters.json
```