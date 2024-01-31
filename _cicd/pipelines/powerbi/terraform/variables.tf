## define variables
variable "storage_account_name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "container_name" {
  type = string
}

variable "resource_location" {
  type = string
}

variable "tf_backend_state" {
  type = object({
    resource_group_name  = string
    storage_account_name = string
    container_name       = string
    key                  = string
  })
}

variable "aad_identity_ids" {
  type = object({
    adf_mi_id      = string
    aegis_group_id = string
  })
}
