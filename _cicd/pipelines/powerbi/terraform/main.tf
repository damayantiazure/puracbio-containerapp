terraform {
  ## find out why we are getting this warning that the backend is needed here. 
  ## we are using this in the pipeline task already. 
  backend "azurerm" {
    resource_group_name  = var.tf_backend_state.resource_group_name
    storage_account_name = var.tf_backend_state.storage_account_name
    container_name       = var.tf_backend_state.container_name
    key                  = var.tf_backend_state.key
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.0.0"
    }
  }
}

provider "azurerm" {
  features {}
}

## create resource group
resource "azurerm_resource_group" "resource_group" {
  name     = var.resource_group_name
  location = var.resource_location
}

resource "azurerm_storage_account" "storage_account" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.resource_group.name
  location                 = azurerm_resource_group.resource_group.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  enable_https_traffic_only = true

  # tags are needed otherwise the policy will not allow the creation of the resources.
  tags = {
    AcceptedException_storage-H-004 = ""
    AcceptedException_storage-H-005 = ""
  }

  network_rules {
    default_action = "Allow"
    bypass         = ["None"]
  }
}

resource "azurerm_storage_container" "container" {
  name                  = var.container_name
  storage_account_name  = azurerm_storage_account.storage_account.name
  container_access_type = "private"
}

resource "azurerm_role_assignment" "adf_role_assignment" {
  scope                = azurerm_storage_account.storage_account.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.aad_identity_ids.adf_mi_id
}

resource "azurerm_role_assignment" "aegis_role_assignment" {
  scope                = azurerm_storage_account.storage_account.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = var.aad_identity_ids.aegis_group_id
}
