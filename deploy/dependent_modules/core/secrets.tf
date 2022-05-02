data "azurerm_resource_group" "foundation_cicd" {
  name = var.foundation_cicd_resource_group_name
}

data "azurerm_key_vault" "secrets" {
  name                = var.foundation_cicd_vault_name
  resource_group_name = data.azurerm_resource_group.foundation_cicd.name
}
data "azurerm_key_vault_secret" "cf_secrets" {
  for_each = toset([
    "HSDP-CF-API-URL",
    "HSDP-CF-DEPLOY-USER",
    "HSDP-CF-DEPLOY-PASSWORD"
  ])

  name         = upper("${var.region}-${each.key}")
  key_vault_id = data.azurerm_key_vault.secrets.id
}