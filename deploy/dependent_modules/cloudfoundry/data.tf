data "cloudfoundry_space_quota" "cf_quota" {
  name = var.cf_quota_name
}

data "cloudfoundry_org" "cf_org" {
  name = var.cf_org
}