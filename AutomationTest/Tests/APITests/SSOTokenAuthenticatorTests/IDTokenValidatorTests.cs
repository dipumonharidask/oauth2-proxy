using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities;
using IdentityModel.Jwk;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITests
{
    [TestClass]
    public class IDTokenValidatorTests : BaseTest
    {
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.IDTokenValidator))]
        [TestCategory(nameof(TestCategory.UpgradeSanity))]
        [TestCategory(nameof(TestCategory.IntegratedSanity))]
        [TestMethod]
        public void GetOpenIdConfigurationAndVerifyJWKSDataTest()
        {

            var openIdConfigOrgs = JsonConvert.DeserializeObject<List<OpenIdConfigOrganizationCertificateMapping>>(pipelineConfigs.OpenIdConfigOrganizationCertificateMapping);

            Report.Step(@"Get Jwks url from OpenId Configuration get call for multitenant ", @"Should get the respective tenant Jwks url");
            foreach (var org in openIdConfigOrgs)
            {
                string openIdConfigUrl = $"{pipelineConfigs.OpenIdConfigurationBaseUrl}{appConfigs.OpenIdConfigurationUrlPath.Replace("OrgId", org.OrganizationId)}";
                Logger.Info($"OpenId Configuration url: {openIdConfigUrl}");
                var openIdConfigResponse = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, openIdConfigUrl, new Dictionary<string, string>(), null);
                string jwksUrl = openIdConfigResponse["jwks_uri"]?.ToString();
                Logger.Info($"Jwks Url: {jwksUrl}");
                AssertTest.IsTrue(!string.IsNullOrWhiteSpace(jwksUrl) && jwksUrl.Contains($"/{org.OrganizationId}/"), failMsg: $"Failed to get Jwks url for the orgId: {org}", passMsg: "Jwks url is fetched successfully");

                Report.Step(@"Get Jwks keys from the get call of jwks url", @"All the Jwks keys should not be null");
                var jwksResponse = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, jwksUrl, new Dictionary<string, string>(), null);
                var jwksData = JsonConvert.DeserializeObject<JsonWebKey>(jwksResponse["keys"]?.First.ToString());

                string expectedCert = RemoveCertificateBoundaryAndLineBreaks(org.Certificate);
                AssertTest.IsTrue(jwksData.X5c != null && jwksData.X5c[0].Equals(expectedCert), failMsg: $"Property 'x5c' value is null ", passMsg: $"Property 'x5c(certificate)' value is matching with expected value");
                AssertTest.IsTrue(jwksData.Kid != null, failMsg: $"Property 'Kid' value is null ", passMsg: $"Property 'Kid'  is not null ");
                AssertTest.IsTrue(jwksData.Kty != null, failMsg: $"Property 'Kty' value is null ", passMsg: $"Property 'Kty' is not null ");
                AssertTest.IsTrue(jwksData.Alg != null, failMsg: $"Property 'alg' value is null ", passMsg: $"Property 'alg' is not null ");
                AssertTest.IsTrue(jwksData.Use != null, failMsg: $"Property 'Use' value is null ", passMsg: $"Property 'Use' is not null ");
                AssertTest.IsTrue(jwksData.N != null, failMsg: $"Property 'N' value is null ", passMsg: $"Property 'N' is not null ");
                AssertTest.IsTrue(jwksData.E != null, failMsg: $"Property 'E' value is null ", passMsg: $"Property 'E' is not null ");
            }
        }

        #region private methods
        private string RemoveCertificateBoundaryAndLineBreaks(string certData)
        {
            return certData.Replace("-----BEGIN CERTIFICATE-----", string.Empty)
                 .Replace("-----END CERTIFICATE-----", string.Empty)
                 .Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", string.Empty);
        }
        #endregion
    }
}
