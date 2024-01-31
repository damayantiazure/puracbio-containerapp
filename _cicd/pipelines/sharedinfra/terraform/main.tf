terraform {  
  backend "azurerm" {
    resource_group_name = var.tf_backend_state.resource_group_name
    storage_account_name = var.tf_backend_state.storage_account_name
    container_name = var.tf_backend_state.container_name
    key = var.tf_backend_state.key
  }

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

provider "azurerm" {
  features {}
}

# Create Azure Monitor infra (resourcegroup, workspace, tables, dce's and dcr's)
module "azuremonitor-infra" {
  source  = "./modules/azuremonitor-infra"
  tablesToCreate = var.tables
  data_collection_endpoint_name_prefix = var.data_collection_endpoint_name_prefix
  data_collection_rule_name_prefix = var.data_collection_rule_name_prefix
  azureDataAzdoCompliancyGroupId = var.azureDataAzdoCompliancyGroupId
  workspaceId = var.workspaceId
  logAnalyticsResourceGroupId = var.logAnalyticsResourceGroupId
}