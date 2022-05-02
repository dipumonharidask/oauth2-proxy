resource "cloudfoundry_space" "envoy_gateway_cicd" {
  name      = var.cf_space_name
  org       = data.cloudfoundry_org.cf_org.id
  quota     = data.cloudfoundry_space_quota.cf_quota.id
  allow_ssh = true
}

resource "cloudfoundry_space_users" "envoy_gateway_cicd_users" {
  space = cloudfoundry_space.envoy_gateway_cicd.id
  managers = [
    var.cf_deploy_user
  ]
  developers = [
    var.cf_deploy_user
  ]
}

resource "cloudfoundry_user_provided_service" "envoy_gateway_cicd_logdrainer" {
  name             = "envoy-gateway-cicd-logdrainer"
  space            = cloudfoundry_space.envoy_gateway_cicd.id
  syslog_drain_url = var.logdrainer_uri

  depends_on = [
    cloudfoundry_space.envoy_gateway_cicd, # it tries to query the space resource inside this module before creating the space
    cloudfoundry_space_users.envoy_gateway_cicd_users
  ]
}