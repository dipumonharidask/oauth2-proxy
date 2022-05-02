using System.Collections.Generic;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Models
{
    public class PipelineConfiguration
    {
        public string AuthUserName { get; set; }
        public string AuthPassword { get; set; }
        public string APIGatewayBaseUrl { get; set; }
        public string OauthClientID { get; set; }
        public string OauthClientSecret { get; set; }
        public string CFOrgName { get; set; }
        public string CFSpaceName { get; set; }
        public string CFUserName { get; set; }
        public string CFPassword { get; set; }
        public string CookieName { get; set; }
        public string CFOauthTokenUrl { get; set; }
        public string CFBaseUrl { get; set; }
        public string CFAuthenticatorAppName { get; set; }
        public int OauthProxyCookieTimeoutInSeconds { get; set; }
        public string AuthenticatorSessionExpireOffsetInPercent { get; set; }
        public string AccessTokenTestRoleName { get; set; }
        public string IAMAuthorizationUrl { get; set; }
        public string IAMAccessTokenUrl { get; set; }
        public string ServiceID { get; set; }
        public string ServiceIDPrivateKey { get; set; }
        public string IDMClientBaseUrl { get; set; }
        public string OrgSymmetricKey { get; set; }
        public string CDRBaseUrl { get; set; }
        public string IAMGetUserUrl { get; set; }
        public string OpenIdConfigurationBaseUrl { get; set; }
        public string SSOTokenUserName { get; set; }
        public string OpenIdConfigOrganizationCertificateMapping { get; set; }
        public string TenantDetails { get; set; }
    }
}
