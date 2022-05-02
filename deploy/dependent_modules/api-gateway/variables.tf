terraform {
  experiments = [module_variable_optional_attrs]
}

variable "CF_API_URL" {
  type        = string
  description = "URL used to connect to CF environment API"
  default     = "https://api.cloud.pcftest.com"
}

variable "proposition_id" {
  type        = string
  description = "foundation proposition id"
}

variable "cf_space" {
  type        = string
  description = "Cloud foundry space"
}

variable "CF_USER" {
  type        = string
  description = "Cloud foundry API username"
  default     = "solutionaccelerator-cicd-svc"

}
variable "cf_org" {
  type        = string
  description = "Cloud foundry API ORG"

}

variable "cf_deploy_user" {
  type        = string
  description = "Cloud foundry domain to deploy api gateway"
}

variable "cf_deploy_password" {
  type        = string
  description = "Cloud foundry domain to deploy api gateway"
}

variable "cf_domain" {
  type        = string
  description = "Cloud foundry domain to deploy api gateway"
}

variable "DOCKER_REGISTRY" {
  type        = string
  description = "Docker registry for all images"
  default     = "docker.na1.hsdp.io/edi"
}


variable "api_gateway_appname" {
  type        = string
  description = "space name"
  default     = "api_gateway"
}


variable "oauth_proxy_appname" {
  type        = string
  description = "Application name"
  default     = "oauth_proxy"
}

variable "envoy_tag" {
  type        = string
  description = "Tag Value for Envoy Gateway"
}

variable "oauth_tag" {
  type        = string
  description = "Tag Value for OAuth Proxy"
}

variable "authenticator_tag" {
  type        = string
  description = "Tag Value for Token Authenticator"
}

variable "tokenexchange_tag" {
  type        = string
  description = "Tag Value for Token Exchange Broker"
}

variable "hsdp_iam_url" {
  type        = string
  description = "The IAM url for the region that the IAM ORG is in"
  default     = "https://iam-client-test.us-east.philips-healthsuite.com"
}

variable "hsdp_idm_url" {
  type        = string
  description = "The IDM url for the region that the IAM ORG is in"
  default     = "https://idm-client-test.us-east.philips-healthsuite.com"
}

variable "client_id" {
  type        = string
  description = "Client ID for terrafom iam"
  default     = "sal_terraform"

}

variable "config_file_location" {
  type        = string
  description = "Envoy Config File for routing"

}

variable "service_id" {
  type        = string
  description = "iam service id - used to configure hsdp provider - service must have oauth2 client creation permission"
  default     = "pf-automation-tf.pf-automation-tf.pf-automation-tf@edi-platform-service.ediplatform.philips-healthsuite.com"
}

variable "syslog_url" {
  type        = string
  default     = "https://logdrainer-client-test.us-east.philips-healthsuite.com/core/log/Product/e66380ff07c42b67483a3eb14e49c8677d5785467f21ac2b1bdfdfa6d7bce9e6423ae79fe42d0456419653ee80222792"
  description = "The URL used for the syslog service created to send logs from each app"
}

variable "org_id_source" {
  type        = string
  description = "org_id source to read from"
}

variable "logdrainer"{
  type        = string
  description = "log drainer service id"
}
variable "oauth_proxy_redis"{
  type        = string
  description = "oauth proxy redis id"
}



variable "redis_credentials" {
  type        = map(any)
  description = "cf redis credentials"
}