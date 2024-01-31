data_collection_endpoint_name_prefix  = "dce-prd"
data_collection_rule_name_prefix      = "dcr-prd"

tf_backend_state = {
  resource_group_name  = "reg-prd-eu-tas-tfstate-rg"
  storage_account_name = "tfbackendstateprdsa"
  container_name       = "tfstateprd"
  key                  = "production/loganalyticsinfra.tfstate"
}