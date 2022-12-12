data "cloudfoundry_service" "redis_service" {
  name = "hsdp-redis-db"
}

resource "cloudfoundry_service_instance" "redis_cicd" {
  name         = "redis-cicd"
  space        = cloudfoundry_space.envoy_gateway_cicd.id
  service_plan = data.cloudfoundry_service.redis_service.service_plans["${var.redis_plan_name}"]

  depends_on = [
    cloudfoundry_space.envoy_gateway_cicd, # it tries to query the space resource inside this module before creating the space
    cloudfoundry_space_users.envoy_gateway_cicd_users
  ]

}

resource "cloudfoundry_service_key" "redis_key" {
  name             = "redis-key"
  service_instance = cloudfoundry_service_instance.redis_cicd.id

  depends_on = [
    cloudfoundry_space.envoy_gateway_cicd, # it tries to query the space resource inside this module before creating the space
    cloudfoundry_space_users.envoy_gateway_cicd_users
  ]

}