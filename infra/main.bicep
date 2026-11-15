param environmentCode string = 'prod'
param location string = resourceGroup().location

resource sql 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'sql-orders-${environmentCode}'
  location: location
  properties: { administratorLogin: 'ordersadmin' }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'stordersinvoices${environmentCode}'
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
}
