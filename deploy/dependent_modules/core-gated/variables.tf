variable "edisp_azure_vault_name" {
  type    = string
  default = "The name of the vault created by the EDI Platform"
}

variable "edisp_azure_automation_account_name" {
  type    = string
  default = "The name of the vault created by the EDI Platform"
}

variable "edisp_resource_group_name" {
  type        = string
  description = "The name of the resoruce group in Azure that holds the resources for EDI Platform"
  default     = "edi-platform"
}

variable "foundation_cicd_resource_group_name" {
  type        = string
  description = "The name of the resoruce group in Azure that holds the resources for EDI Platform foundation CICD"
  default     = "edi-platform-foundation-cicd"
}

variable "foundation_cicd_vault_name" {
  type        = string
  description = "The name of the resoruce group in Azure that holds the resources for EDI Platform foundation CICD"
  default     = "foundationcicd"
}

variable "region" {
  type        = string
  description = "HSDP region that this will be deployed to"
}

variable "environment" {
  type        = string
  description = "HSDP environment that this will be deployed to (one of client-test or prod)"
}