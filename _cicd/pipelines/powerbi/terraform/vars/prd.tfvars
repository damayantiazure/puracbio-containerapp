resource_location    = "westeurope"
container_name       = "department-hierarchy"
resource_group_name  = "reg-prd-eu-tas-azdocompliancy-powerbireports"
storage_account_name = "powerbireportsprdsa"
aad_identity_ids = {
  adf_mi_id      = "1e62f6eb-c128-41be-a53c-bbcfe02b55d2" # managed identity for the data reporting team adf instance
  aegis_group_id = "5fb4e1cc-ab53-40ed-a1e3-14f39b776146" # group id for team aegis application role
}
tf_backend_state = {
  resource_group_name  = "reg-prd-eu-tas-tfstate-rg"
  storage_account_name = "tfbackendstateprdsa"
  container_name       = "tfstateprd"
  key                  = "terraform.tfstate"
}
