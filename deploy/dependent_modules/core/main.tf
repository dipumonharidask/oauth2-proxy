module "edisp" {
  source = "github.com/philips-internal/terraform-edi-platform-secrets?ref=v0.1.2"

  region                              = var.region
  environment                         = var.environment
  edisp_azure_vault_name              = var.edisp_azure_vault_name
  edisp_resource_group_name           = var.edisp_resource_group_name
  edisp_azure_automation_account_name = var.edisp_azure_automation_account_name
  variables                           = ["TENANTS", "ORG-ID", "EDI-ORG-ID", "EDIPLATFORM-LOGGING-LOGDRAINER-BASE-URI"]
}

data "hsdp_iam_org" "proposition_org" {
  organization_id = module.edisp.variables["ORG-ID"]
}

resource "hsdp_iam_proposition" "foundation_envoy_cicd_prop" {
  name            = "FOUNDATION-OAUTH-CI-TF"
  description     = "Proposition id Created for Envoy CD through terraform"
  organization_id = hsdp_iam_org.foundation_envoy_nightly_org.id
}

resource "hsdp_iam_org" "foundation_envoy_nightly_org" {
  name          = "Oauth-CI-Org-TF"
  description   = "Envoy Nightly Organization to run envoy nightly tests, Do not modify manually"
  parent_org_id = data.hsdp_iam_org.proposition_org.id
}

resource "hsdp_iam_application" "foundation_envoy_nightly_app" {
  name           = "OAUTH-CI-APP-TF"
  description    = "Envoy CI application for Automation Tests, Do not modify manually"
  proposition_id = hsdp_iam_proposition.foundation_envoy_cicd_prop.id
}

resource "hsdp_iam_service" "envoy_ci_service" {
  name           = "oauth-cicd-service"
  description    = "Service Client for Envoy Nightly Tests, Do not modify manually"
  application_id = hsdp_iam_application.foundation_envoy_nightly_app.id

  validity = 12

  scopes         = ["openid"]
  default_scopes = ["openid"]
}

resource "hsdp_iam_role" "envoy_ci_service_role" {

  managing_organization = hsdp_iam_org.foundation_envoy_nightly_org.id
  name                  = "OAUTH-CI-SERVICE-TF"
  description           = "Permissions to create IAM resources below the provided proposition."
  permissions = [
    "GROUP.READ",
    "GROUP.WRITE",
    "USER.READ",
    "USER.WRITE",
    "BASIC.WRITE",
    "ROLE.READ",
    "ROLE.WRITE",
    "PERMISSION.READ",
    "BASICTEST.WRITE"
  ]
}

resource "hsdp_iam_group" "envoy_ci_service_group" {

  managing_organization = hsdp_iam_org.foundation_envoy_nightly_org.id
  name                  = "OAUTH-CI-SERVICE-TF"
  description           = "Group for envoy automation service identities, Do not modify manually"
  services              = [hsdp_iam_service.envoy_ci_service.id]
  roles                 = [hsdp_iam_role.envoy_ci_service_role.id]
}

module "hsdp_iam_foundation_auto_oauth2_client" {
  source = "github.com/philips-internal/terraform-module-iam-client?ref=v0.1.0"

  client_name      = "oauth_ci_client"
  client_id        = "oauth-ci"
  scopes           = ["mail", "sn", "profile", "auth_iam_organization", "auth_iam_introspect"]
  response_types   = ["code"]
  redirection_uris = []
  application_id   = hsdp_iam_application.foundation_envoy_nightly_app.id
}