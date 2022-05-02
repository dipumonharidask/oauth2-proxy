include {
  path = find_in_parent_folders()
}

terraform {
  source = "../../dependent_modules//core"
}

inputs = {
  edisp_azure_vault_name              = "edisp-UB5gKdbmC3"
  edisp_azure_automation_account_name = "edisp-HPTWRpsLcl"
}