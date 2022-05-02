output "logdrainer_service_id" {
  value = cloudfoundry_user_provided_service.envoy_gateway_cicd_logdrainer.id
}

output "redis_service_id" {
  value = cloudfoundry_service_instance.redis_cicd.id
}

output "redis_credentials" {
  value = cloudfoundry_service_key.redis_key.credentials
}