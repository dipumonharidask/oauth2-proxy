locals {

  monteback_routes = [{
    route = cloudfoundry_route.monteback.id
    }
  ]
}


module "app_api_gateway" {
 source = "github.com/philips-internal/terraform-api-gateway?ref=v0.0.32"

  api_gateway_image           = { "image_name" : "edi-foundation-envoy-gateway", "registry" : "docker.na1.hsdp.io/edi", "image_tag" : "${var.envoy_tag}", "registry_username" : "${var.cf_deploy_user}", "registry_password" : "${var.cf_deploy_password}" }
  oauth2_proxy_image          = { "image_name" : "edi-foundation-oauth2-proxy", "registry" : "docker.na1.hsdp.io/edi", "image_tag" : "${var.oauth_tag}", "registry_username" : "${var.cf_deploy_user}", "registry_password" : "${var.cf_deploy_password}" }
  authenticator_service_image = { "image_name" : "edi-foundation-apigateway-tokenauthenticator", "registry" : "docker.na1.hsdp.io/edi", "image_tag" : "${var.authenticator_tag}", "registry_username" : "${var.cf_deploy_user}", "registry_password" : "${var.cf_deploy_password}" }
  tokenexchange_broker_image =  { "image_name" : "edi-foundation-iam-tokenexchange-broker", "registry" : "docker.na1.hsdp.io/edi", "image_tag" : "${var.tokenexchange_tag}", "registry_username" : "${var.cf_deploy_user}", "registry_password" : "${var.cf_deploy_password}" }
  api_gateway_routes = [
    {
      hostname = "${data.cloudfoundry_space.cf_space.name}-api-gateway"
      port     = 8080
    },
    {
      hostname = "org1-${data.cloudfoundry_space.cf_space.name}-api-gateway"
      port     = 8080
    },
    {
      hostname = "invalidorg-${data.cloudfoundry_space.cf_space.name}-api-gateway"
      port     = 8080
    }
  ]
  envoy_config              = base64encode(file(var.config_file_location))
  space                     = var.cf_space
  proposition_id            = var.proposition_id
  cf_org                    = var.cf_org
  domain                    = var.cf_domain
  api_gateway_environment = {
    "MOUNTEBANK_ENDPOINT"      = "${cloudfoundry_route.monteback.endpoint}"
    "MOUNTEBANK_PORT"          = 2525
    "MONTEBANKSERVICEDOWNTEST" = "${cloudfoundry_route.montebankservicedowntest.endpoint}"
    "ORG_ID_SOURCE"            = var.org_id_source
    "ORG_ID_MAPPING"           = "{[\"org1-${data.cloudfoundry_space.cf_space.name}-api-gateway\"] = \"51e2503f-c1df-430d-a1ce-2524fa796cda\"}"
    "TimeoutInSeconds"         = "0.750s"
    "ENVOY_PORT"               = 8080   
  }
  oauth2_environment = {
    "OAUTH2_PROXY_COOKIE_REFRESH"       = "0h0m25s"
    "OAUTH2_PROXY_SESSION_STORE_TYPE"   = "redis"
    "OAUTH2_PROXY_REDIS_CONNECTION_URL" = "redis://:${var.redis_credentials["password"]}@${var.redis_credentials["hostname"]}:${var.redis_credentials["port"]}"
  }
  logout_redirect_url = "https://${data.cloudfoundry_space.cf_space.name}-api-gateway.${var.cf_domain}"
  log_service_id      = var.logdrainer
  redis_service_id    = var.oauth_proxy_redis
  vault_service_id    = cloudfoundry_service_instance.gateway_vault.id
  use_authenticator   = "true"
  use_tokenexchangebroker   = "true"
}




resource "cloudfoundry_network_policy" "monteback_a" {
  policy {
    source_app      = module.app_api_gateway.api_gw_application_id
    destination_app = cloudfoundry_app.monteback.id
    port            = 2525
  }
}

resource "cloudfoundry_network_policy" "monteback_b" {
  policy {
    source_app      = module.app_api_gateway.api_gw_application_id
    destination_app = cloudfoundry_app.monteback.id
    port            = 4545
  }
}

resource "cloudfoundry_network_policy" "monteback_c" {
  policy {
    source_app      = module.app_api_gateway.api_gw_application_id
    destination_app = cloudfoundry_app.monteback.id
    port            = 4546
  }
}

resource "cloudfoundry_network_policy" "monteback_d" {
  policy {
    source_app      = module.app_api_gateway.api_gw_application_id
    destination_app = cloudfoundry_app.monteback.id
    port            = 4550
  }
}

resource "cloudfoundry_network_policy" "monteback_FilterService" {
  policy {
    source_app      = module.app_api_gateway.api_gw_application_id
    destination_app = cloudfoundry_app.monteback.id
    port            = 4547
  }
}

resource "cloudfoundry_app" "monteback" {
  space        = data.cloudfoundry_space.cf_space.id
  name         = "monteback"
  docker_image = "${var.DOCKER_REGISTRY}/mountebank:latest"
  instances    = 1
  memory       = 300
  disk_quota   = 512
  command      = "node bin/mb --allowInjection"
  docker_credentials = {
    username = var.cf_deploy_user
    password = var.cf_deploy_password
  }
  dynamic "routes" {
    for_each = local.monteback_routes

    content {

      route = routes.value.route
    }
  }

}


resource "cloudfoundry_route" "monteback" {
  domain   = data.cloudfoundry_domain.internal.id
  space    = data.cloudfoundry_space.cf_space.id
  hostname = "montebanktest-${data.cloudfoundry_space.cf_space.name}"

}

resource "cloudfoundry_route" "montebankservicedowntest" {
  domain   = data.cloudfoundry_domain.internal.id
  space    = data.cloudfoundry_space.cf_space.id
  hostname = "montebankservicedowntest-${data.cloudfoundry_space.cf_space.name}"

}
resource "cloudfoundry_service_instance" "gateway_vault" {
  name         = "gateway_vault"
  space        = data.cloudfoundry_space.cf_space.id
  service_plan = data.cloudfoundry_service.vault.service_plans["vault-us-east-1"]
}