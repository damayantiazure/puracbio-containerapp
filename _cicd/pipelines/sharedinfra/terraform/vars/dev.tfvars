data_collection_endpoint_name_prefix  = "dce-dev"
data_collection_rule_name_prefix      = "dcr-dev"

tf_backend_state = {
  resource_group_name  = "reg-dev-eu-devaut-tfstate-rg"
  storage_account_name = "tfbackendstatedevsa"
  container_name       = "tfstatedev"
  key                  = "development/loganalyticsinfra.tfstate"
}