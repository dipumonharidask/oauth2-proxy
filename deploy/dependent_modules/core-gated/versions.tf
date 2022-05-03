terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.67.0"
    }
    hsdp = {
      source  = "philips-software/hsdp"
      version = ">= 0.19.5"
    }
  }
  required_version = ">= 1.0"
}