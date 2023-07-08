az cosmosdb sql role definition create \
    --account-name my-life-cosmos \
    --resource-group  my-life \
    --body '{
    "RoleName": "PasswordlessReadWrite",
    "Type": "CustomRole",
    "AssignableScopes": ["/"],
    "Permissions": [{
        "DataActions": [
            "Microsoft.DocumentDB/databaseAccounts/readMetadata",
            "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*",
            "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*"
        ]
    }]
}'