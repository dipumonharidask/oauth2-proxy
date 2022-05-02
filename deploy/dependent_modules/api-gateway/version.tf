terraform {
  required_providers {
    cloudfoundry = {
      source  = "cloudfoundry-community/cloudfoundry"
      version = ">= 0.14.2"
    }
    hsdp = {
      source  = "philips-software/hsdp"
      version = ">= 0.19.5"
    }
  }
  required_version = ">= 1.0"
}
