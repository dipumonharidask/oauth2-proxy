

data "cloudfoundry_org" "org" {
  name = var.cf_org
}

data "cloudfoundry_space" "cf_space" {
  name = var.cf_space
  org  = data.cloudfoundry_org.org.id
}

data "cloudfoundry_domain" "internal" {
  name = "apps.internal"
}


data "cloudfoundry_service" "data_redis_service" {
  name = "hsdp-redis-db"
}


data "cloudfoundry_service" "vault" {
  name = "hsdp-vault"
}