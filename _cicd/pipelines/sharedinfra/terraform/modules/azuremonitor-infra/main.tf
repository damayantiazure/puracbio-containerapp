 terraform {
    required_providers {
        azurerm = {
            source  = "hashicorp/azurerm"
            version = "=3.0.0"
        }
            azapi = {
            source = "azure/azapi"
        }
    }
}

# Create tables
resource "azapi_resource" "table_creation" {
  for_each = { for table in var.tablesToCreate.tableDefs : table.name => table }
  type = "Microsoft.OperationalInsights/workspaces/tables@2022-10-01"
  name = each.key
  parent_id = var.workspaceId  
  lifecycle {
    # prevent_destroy = true
    ignore_changes = [      
      id, parent_id, tags
    ]
  }

  body = jsonencode(
    {
      "properties" : {
        "schema" : {
          "name" : each.key,
          "columns" : each.value.columns        
        }        
      }
    }
  )
}

# Create the data collection endpoint
resource "azapi_resource" "data_collection_endpoint_compliancy_data" {  
  type = "Microsoft.Insights/dataCollectionEndpoints@2021-09-01-preview"
  name = format("%s-compliancy-data", var.data_collection_endpoint_name_prefix)
  location = "westeurope"
  parent_id = var.logAnalyticsResourceGroupId
  body = jsonencode({
    properties = {      
      description = "Endpoint for ingestion of data"
      networkAcls = {
        publicNetworkAccess = "Enabled"
      }
    }
    kind = "Windows"
  })
}

# Create the data collection rules for each table and assign to previously created endpoint
resource "azapi_resource" "data_collection_rule_compliancy_data" {
  for_each = { for table in var.tablesToCreate.tableDefs : table.name => table }
  type = "Microsoft.Insights/dataCollectionRules@2021-09-01-preview"
  name = format("%s-%s", var.data_collection_rule_name_prefix, replace(each.key,"_","-"))
  location = "westeurope"
  parent_id = var.logAnalyticsResourceGroupId
  depends_on = [
    azapi_resource.table_creation
  ]
  body = jsonencode({
    properties = {      
      dataCollectionEndpointId = azapi_resource.data_collection_endpoint_compliancy_data.id
      description = format("Rule for ingestion of data into %s table", each.key)
      destinations = {
        logAnalytics = [
          {
            name = format("%s", each.key)
            workspaceResourceId = var.workspaceId
          }
        ]
      },
      dataFlows = [
          {
              "streams": [
                  format("Custom-%s", each.key)
              ],
              "destinations": [
                  format("%s", each.key)
              ],              
              "outputStream": format("Custom-%s", each.key)
          }
      ]
      streamDeclarations = {          
          format("Custom-%s", each.key): {
              "columns" : each.value.columns
          }
      }
    }
  })
}

# Do role assignments
resource "azurerm_role_assignment" "azuredata_publisher_role_assignment" {
  for_each = { for table in var.tablesToCreate.tableDefs : table.name => table }
  scope                = azapi_resource.data_collection_rule_compliancy_data[each.key].id
  role_definition_name = "Monitoring Metrics Publisher"
  principal_id         = var.azureDataAzdoCompliancyGroupId
}

resource "azurerm_role_assignment" "azuredata_reader_role_assignment" {
  for_each = { for table in var.tablesToCreate.tableDefs : table.name => table }
  scope                = azapi_resource.data_collection_rule_compliancy_data[each.key].id
  role_definition_name = "Monitoring Reader"
  principal_id         = var.azureDataAzdoCompliancyGroupId
}