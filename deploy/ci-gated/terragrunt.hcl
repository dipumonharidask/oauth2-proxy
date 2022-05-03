skip = true

locals {
  path_segments         = split("/", path_relative_to_include())
  environment           = "client-test"
  region                = "us-east"
  azure_subscription_id = "74d03f75-4bdb-4666-8095-500178a40764"
}

remote_state {
  backend = "azurerm"
  generate = {
    path      = "backend.tf"
    if_exists = "overwrite_terragrunt"
  }
  config = {
    storage_account_name = "foundationcicd"
    container_name       = "envoyci-gated"
    key                  = "${path_relative_to_include()}/terraform.tfstate"
    resource_group_name  = "edi-platform-foundation-cicd"
    subscription_id      = "${local.azure_subscription_id}"
    use_azuread_auth     = true
  }
}

// Setup the region and environment based on the folder name

generate "auto_vars" {
  path      = "terragrunt.auto.tfvars"
  if_exists = "overwrite_terragrunt"
  contents  = <<EOF
region      = "${local.region}"
environment = "${local.environment}"
EOF
}


terraform {
  extra_arguments "azure_config" {
    commands = get_terraform_commands_that_need_vars()

    env_vars = {
      # other credentials are secrets and are passed as env vars in the pipeline
      # or local credentails are used when logging in as a user
      ARM_SUBSCRIPTION_ID = local.azure_subscription_id
    }
  }
}