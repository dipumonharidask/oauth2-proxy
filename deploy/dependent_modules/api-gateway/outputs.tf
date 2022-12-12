output "iam_url_us_east_prod1" {
  value = module.app_api_gateway.iam_url
}

output "api_gw_endpoints" {
  value = module.app_api_gateway.api_gw_proposition_endpoints
}
