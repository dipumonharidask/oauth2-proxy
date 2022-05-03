provider "azurerm" {
  features {}
  # This provider is configuered by environment secrets managed by the calling configuration
}

provider "hsdp" {
  region              = var.region
  environment         = var.environment
  service_id          = module.edisp.secrets["SERVICE-ID"]
  service_private_key = module.edisp.secrets["SERVICE-KEY"]
  uaa_username        = data.azurerm_key_vault_secret.cf_secrets["HSDP-CF-DEPLOY-USER"].value
  uaa_password        = data.azurerm_key_vault_secret.cf_secrets["HSDP-CF-DEPLOY-PASSWORD"].value
}