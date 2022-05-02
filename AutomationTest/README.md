# API Gateway Automation

## Pre-Condition

Mountebank application should be deployed along with API Gateway deployment. 
Mountebank deployment with terrform: https://github.com/philips-internal/hds-auth-gateway/blob/master/Automation/deploy/cloud/app_monteback.tf

Allow Injection command should be passed while starting the mountebank application

command = "node bin/mb --allowInjection"

PostDeploymentTest has to executed first in order to create less payload and heavy payload mockservices for automation. 
(TestCategory = PostDeployment)

Mockservice configuration files https://github.com/philips-internal/hds-auth-gateway/tree/master/AutomationTest/Tests/Data/MockServiceConfigs

## Automation Configuration

https://github.com/philips-internal/hds-auth-gateway/blob/master/AutomationTest/Env.json

AppConfiguration section is for static confguration values and drive name can be changed from C:\ to other drive for automation word reports and screenshots.
  
   "AppConfiguration": { }

PipelineConfiguration section is for dynamic configuration which will be filled from tfs pipeline or user can fill before executing the automation.

  "PipelineConfiguration": { }


| Key             | Description                                                                                                                                                                                                                    | Type |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| AuthUserName     | IAM login user email address, For browser based login tests and also for access token authenticator tests                                                                                                                                                                               | String    |
| AuthPassword     | IAM login password                                                                                                                                                                                     | String    |
| APIGatewayBaseUrl     | BaseUrl fo the API gateway <br/>Examples:<br/>For Cloud: https://covid-ver2-api-gateway.us-east.philips-healthsuite.com<br/>For On-Prem: https://localhost (or) https://hostname                                                                                                                                                                                    | String    |
| OauthProxyCookieTimeoutInSeconds     | Oauth proxy cookie time out in seconds ( default value: 15 )                                                                                                                                                                                     | Integer    |
| TenantDetails     | Tenant information with user-friendly tenant sub-domain name and its relevant Iam organization-ID. <br/>Default Values:<br/>"ValidTenant":{"tenantName":"","iamOrganizationId":""}<br>"InvalidTenant":{"tenantName":"invalidtenant","iamOrganizationId":""}<br>"AccessTokenTenant":{"tenantName":"accesstokentenant","iamOrganizationId":""}                                                                                                                                                                                    | String    |
| OauthClientID     | IAM Oauth client id or User name. Used to get the Authorization Basic token, which will be used to get User or ServiceId Access token.                                                                                                                                                                                     | String    |
| OauthClientSecret     | IAM Oauth client password                                                                                                                                                                                   | String    |
| CFSpaceName     | Cloud foundry space name where the api gateway is deployed                                                                                                                                                                                    | String    |
| CFUserName     | Cloud foundry user name                                                                                                                                                                                     | String    |
| CFPassword     | Cloud foundry user password                                                                                                                                                                                     | String    |
| CookieName     | OAuth2 Proxy cookie name                                                                                                                                                                                         | String    |
| CFLoginUrl     | Cloud foundry login url (default: https://login.cloud.pcftest.com/oauth/token)                                                                                                                                                                                     | String    |
| CFAppsUrl     | Cloud foundry apps url (default: https://api.cloud.pcftest.com/v3/apps)                                                                                                                                                                                     | String    |
| CFAuthenticatorAppName     | Token Authenticator internal app name deployed in the space                                                                                                                                                                                     | String    |
| AuthenticatorSessionExpireOffsetInPercent     | Redis cache clear timeout offset for Authenticator app (default: 10), eg: if set to 99, then Redis cache will get cleared in 18 seconds ( if IAM access token timeout: 30 minutes)                                                                                                                                                                                    | String    |
| ServiceID     | Service identities ID in IAM                                                                                                                                                                                    | String    |
| ServiceIDPrivateKey     | ServiceID's private key in IAM                                                                                                                                                                                    | String    |
| IDMClientBaseUrl     | IDM client url till identiry (eg: https://idm-client-test.us-east.philips-healthsuite.com/authorize/identity) IAM                                                                                                                                                                                    | String    |
String    |
| IAMAuthorizationUrl     | IAM client authorization url (eg: https://iam-client-test.us-east.philips-healthsuite.com/authorize/oauth2/token) IAM                                                                                                                                                                                    | String    |
| IAMAccessTokenUrl     | IAM client access token url (eg: https://iam-client-test.us-east.philips-healthsuite.com/oauth2/access_token) IAM                                                                                                                                                                                    | String    |
| AccessTokenTestRoleName     | Role with BASIC.READ permission created for an organization ( eg: RoleName: TESTROLE ) which is used for access token authenticator tests                                                                                                                                                                                  | String    |

## Automation Test categories and corresponding Envoy configuration files to deploy

| TestCategory             | Envoy file                                                                                                                                                                                                                    | Comments |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| WithoutMultitenancy     | https://github.com/philips-internal/hds-auth-gateway/tree/master/AutomationTest/Tests/Data/EnvoyConfigs/envoyconfig_without_multitenancy.yaml                                                                                                                                                                                   | With Standalone Redis    |
| OrgNameInUrl     | https://github.com/philips-internal/hds-auth-gateway/tree/master/AutomationTest/Tests/Data/EnvoyConfigs/envoyconfig_with_multitenancy.yaml                                                                                                                                                                                   | With Standalone Redis and APIGateway environment variable ORG_ID_SOURCE="url"    |
| OrgIdInHeader     | https://github.com/philips-internal/hds-auth-gateway/tree/master/AutomationTest/Tests/Data/EnvoyConfigs/envoyconfig_with_multitenancy.yaml                                                                                                                                                                                   | With Cluster Redis and APIGateway environment variable ORG_ID_SOURCE="header"    |
| UserAccessToken     | https://github.com/philips-internal/hds-auth-gateway/tree/master/AutomationTest/Tests/Data/EnvoyConfigs/envoyconfig_withoutMultitenancy_WithAuthenticator.yaml                                                                                                                                                                                   | With Standalone Redis"    |
| ServiceIDAccessToken     | https://github.com/philips-internal/hds-auth-gateway/tree/master/AutomationTest/Tests/Data/EnvoyConfigs/envoyconfig_withoutMultitenancy_WithAuthenticator.yaml                                                                                                                                                                                   | With Standalone Redis"    |

## Automation Test Reports
Automation  test reports will be available as word report and trx report file which is interated with tfs release pipeline and displayed in test results dashboard.
https://tfsemea1.ta.philips.com/tfs/TPC_Region11/SAL/_dashboards/dashboard/76acadfe-3e1f-4225-9f4f-19af794bc95f 

## Manual Test cases suite:
https://tfsemea1.ta.philips.com/tfs/TPC_Region11/Healthcare%20IT/_testPlans/define?planId=1297712&suiteId=1298021