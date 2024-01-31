variable "data_collection_endpoint_name_prefix" {
  type = string
}

variable "data_collection_rule_name_prefix" {
  type = string
}

variable "tables" {
  type = any
}

variable "tf_backend_state" {
  type = object({
    resource_group_name  = string
    storage_account_name = string
    container_name       = string
    key                  = string
  })
}

variable "azureDataAzdoCompliancyGroupId" {
  type = string
}

variable "workspaceId" {
  type = string
}

variable "logAnalyticsResourceGroupId" {
  type = string
}