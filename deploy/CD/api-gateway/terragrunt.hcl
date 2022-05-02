include {
  path = find_in_parent_folders()
}

terraform {
  source = "../../dependent_modules//api-gateway"
}

inputs = {

  proposition_id       = dependency.core.outputs.foundation_envoy_proposition_id
  cf_org               = "client-EDI-SolutionAccelerator"
  cf_space             = "envoyci"
  cf_deploy_user       = dependency.core.outputs.cf_deploy_user
  cf_deploy_password   = dependency.core.outputs.cf_deploy_password
  cf_domain            = "us-east.philips-healthsuite.com"
  redis_credentials    = dependency.cloudfoundry.outputs.redis_credentials
  logdrainer           = dependency.cloudfoundry.outputs.logdrainer_service_id
  oauth_proxy_redis    = dependency.cloudfoundry.outputs.redis_service_id
  org_id_source        = get_env("SOURCE", "url")
  config_file_location = get_env("CONFIG", "./envoyconfig_without_multitenancy.yml")
  envoy_tag            = dependency.core.outputs.envoy_tag
  oauth_tag            = dependency.core.outputs.oauth_tag
  authenticator_tag    = dependency.core.outputs.authenticator_tag
  tokenexchange_tag    = dependency.core.outputs.tokenexchange_tag
}

dependency "core" {
  config_path = "../core/"
}

dependency "cloudfoundry" {
  config_path = "../cloudfoundry/"
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
provider "hsdp" {
  region              = "us-east"
  environment         = "client-test"
  service_id          = "${dependency.core.outputs.automation_service_id}"
  service_private_key = "${replace(dependency.core.outputs.automation_service_private_key, "\n", "")}"
}
EOF
}