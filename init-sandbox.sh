#!/bin/bash

deploymentName="CosmicWorks-$RANDOM"
ResourceGroupName=$deploymentName

az deployment group create \
    --resource-group $resourceGroupName \
    --name $deploymentName \
    --template-file azuredeploy.json

uri=$(az deployment group show \
    --resource-group $resourceGroupName \
    --name $deploymentName \
    --query "properties.outputs.uri.value" \
    --output tsv)

key=$(az deployment group show \
    --resource-group $resourceGroupName \
    --name $deploymentName \
    --query "properties.outputs.key.value" \
    --output tsv)

rm -f "appSettings.json"

appSettings=$(cat << EOF 
{
    "uri": "$uri", 
    "key": "$key"
}
EOF
)
echo "$appSettings" > "appSettings.json"

echo "Resource Group Name" $resourceGroupName
echo "Deployment Name" $deploymentName
echo "URI" $uri
echo "Key" $key

echo "Setup complete"