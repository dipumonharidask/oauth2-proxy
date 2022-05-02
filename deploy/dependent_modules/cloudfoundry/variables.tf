variable "cf_org" {
  type        = string
  description = "Cloud foundry org"
}

variable "cf_quota_name" {
  type        = string
  description = "cloud foundry space quota.(Should be predefined for all the foundation cicd spaces)"
  default     = "foundation-cicd-quota"
}

variable "cf_space_name" {
  type        = string
  description = "cloud foundry space name to be created"
}

variable "cf_deploy_user" {
  type        = string
  description = "cloudfoundry user to assign space permisions"
}

variable "logdrainer_uri" {
  type        = string
  description = "cloudfoundry user to assign space permisions"
}

variable "redis_plan_name" {
  type        = string
  description = "cloudfoundry redis plan name"
}