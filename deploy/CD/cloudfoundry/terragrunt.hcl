include {
  path = find_in_parent_folders()
}

terraform {
  source = "../../dependent_modules//cloudfoundry"
}

inputs = {
  cf_org          = "client-EDI-SolutionAccelerator"
  cf_space_name   = "envoyci"
  logdrainer_uri  = "${dependency.core.outputs.logdrainer_base_uri}${dependency.core.outputs.logdrainer_uri}"
  redis_plan_name = "redis-development-standalone"
  cf_deploy_user  = dependency.core.outputs.cf_deploy_user
}

dependency "core" {
  config_path = "../core"
}

generate "provider" {
  path      = "provider.tf"
  if_exists = "overwrite_terragrunt"
  contents  = <<EOF
provider "cloudfoundry" {
  api_url  = "${dependency.core.outputs.cf_api_url}"
  user     = "${dependency.core.outputs.cf_deploy_user}"
  password = "${dependency.core.outputs.cf_deploy_password}"
}
EOF
}