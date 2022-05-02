output "automation_service_id" {
  value     = module.edisp.secrets["SERVICE-ID"]
  sensitive = true
}

output "automation_service_private_key" {
  value     = module.edisp.secrets["SERVICE-KEY"]
  sensitive = true
}

output "logdrainer_uri" {
  value     = module.edisp.secrets["EDIPLATFORM-LOGGING-LOGDRAINER-URI"]
  sensitive = true
}

output "logdrainer_base_uri" {
  value = module.edisp.variables["EDIPLATFORM-LOGGING-LOGDRAINER-BASE-URI"]
}

output "foundation_cicd_org_id" {
  value = module.edisp.variables["ORG-ID"]
}

output "foundation_envoy_proposition_id" {
  value = hsdp_iam_proposition.foundation_envoy_cicd_prop.id
}

output "ediplatform_org_id" {
  value = module.edisp.variables["EDI-ORG-ID"]
}

output "cf_api_url" {
  value     = data.azurerm_key_vault_secret.cf_secrets["HSDP-CF-API-URL"].value
  sensitive = true
}

output "cf_deploy_user" {
  value     = data.azurerm_key_vault_secret.cf_secrets["HSDP-CF-DEPLOY-USER"].value
  sensitive = true
}

output "cf_deploy_password" {
  value     = data.azurerm_key_vault_secret.cf_secrets["HSDP-CF-DEPLOY-PASSWORD"].value
  sensitive = true
}

output "foundation_envoy_nightly_org_id" {
  value = hsdp_iam_org.foundation_envoy_nightly_org.id
}

output "foundation_envoy_nightly_service_id" {
  value = hsdp_iam_service.envoy_ci_service.service_id
}

output "foundation_envoy_nightly_service_private_key" {
  value     = replace(hsdp_iam_service.envoy_ci_service.private_key, "\n", "")
  sensitive = true
}

output "fdn_envoy_oauth_client_id" {
  value = module.hsdp_iam_foundation_auto_oauth2_client.client_id
}

output "fdn_envoy_oauth_client_password" {
  value     = module.hsdp_iam_foundation_auto_oauth2_client.secret
  sensitive = true
}

output "hsdp_iam_url" {
  value = data.hsdp_config.iam_url.url
}

output "hsdp_idm_url" {
  value = data.hsdp_config.idm_url.url
}

output "envoy_tag" {
  value = data.hsdp_docker_repository.edi-foundation-envoy-gateway.tags[0]
}

output "oauth_tag" {
  value = data.hsdp_docker_repository.edi-foundation-oauth2-proxy.tags[0]
}

output "authenticator_tag" {
  value = data.hsdp_docker_repository.edi-foundation-apigateway-tokenauthenticator.tags[0]
}

output "tokenexchange_tag" {
  value = data.hsdp_docker_repository.edi-foundation-iam-tokenexchange-broker.tags[0]
}