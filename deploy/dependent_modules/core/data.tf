data "hsdp_config" "iam_url" {
  service     = "iam"
  region      = var.region
  environment = var.environment
}

data "hsdp_config" "idm_url" {
  service     = "idm"
  region      = var.region
  environment = var.environment
}

data "hsdp_docker_namespace" "edi" {
  name = "edi"
}

data "hsdp_docker_repository" "edi-foundation-envoy-gateway" {
  namespace_id = data.hsdp_docker_namespace.edi.id
  name         = "edi-foundation-envoy-gateway"
}

data "hsdp_docker_repository" "edi-foundation-oauth2-proxy" {
  namespace_id = data.hsdp_docker_namespace.edi.id
  name         = "edi-foundation-oauth2-proxy"
}

data "hsdp_docker_repository" "edi-foundation-apigateway-tokenauthenticator" {
  namespace_id = data.hsdp_docker_namespace.edi.id
  name         = "edi-foundation-apigateway-tokenauthenticator"
}

data "hsdp_docker_repository" "edi-foundation-iam-tokenexchange-broker" {
  namespace_id = data.hsdp_docker_namespace.edi.id
  name         = "edi-foundation-iam-tokenexchange-broker"
}