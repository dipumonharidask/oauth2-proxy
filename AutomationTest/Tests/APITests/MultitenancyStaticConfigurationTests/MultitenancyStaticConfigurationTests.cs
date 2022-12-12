using Driver.UI.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Utilities;
namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITests.MultitenancyStaticConfiguration
{
    [TestClass]
    public class MultitenancyStaticConfigurationTests : BaseTest
    {

        private string placeHolderForOrgReplacement;

        private Cookies _cookie;
        private Dictionary<string, string> _headers;
        private static string _apiGatewayBaseUrlWithOrgNamePlaceholder;
        private static string _cdrServiceUrlBasePathWithoutOrgId;
        private string _cdrSubscriptionUrlWithOrg1;
        private string _cdrSubscriptionUrlWithInvalidOrgName;
        private string _qidoStudyLevelUrlWithInvalidOrgName;


        [TestInitialize]
        public void BeforeTest()
        {
            Logger.Info("Before test");
            UriBuilder uriBuilder = new UriBuilder(pipelineConfigs.APIGatewayBaseUrl);
            placeHolderForOrgReplacement = $"OrgName-{uriBuilder.Host.Split('.')[0]}";
            uriBuilder.Host = $"OrgName-{uriBuilder.Host}";
            _apiGatewayBaseUrlWithOrgNamePlaceholder = uriBuilder.Uri.AbsoluteUri;
            _cdrServiceUrlBasePathWithoutOrgId = appConfigs.CDRImagingStudyUrlPath.Replace("/OrgId", string.Empty);
            _cdrSubscriptionUrlWithOrg1 = $"{_apiGatewayBaseUrlWithOrgNamePlaceholder.Replace(placeHolderForOrgReplacement, tenantDetails[Constants.ValidTenantKey].TenantName, StringComparison.InvariantCultureIgnoreCase)}{_cdrServiceUrlBasePathWithoutOrgId}";
            _cdrSubscriptionUrlWithInvalidOrgName = $"{_apiGatewayBaseUrlWithOrgNamePlaceholder.Replace(placeHolderForOrgReplacement, tenantDetails[Constants.InvalidTenantKey].TenantName, StringComparison.InvariantCultureIgnoreCase)}{_cdrServiceUrlBasePathWithoutOrgId}";
            _qidoStudyLevelUrlWithInvalidOrgName = $"{_apiGatewayBaseUrlWithOrgNamePlaceholder.Replace(placeHolderForOrgReplacement, tenantDetails[Constants.InvalidTenantKey].TenantName, StringComparison.InvariantCultureIgnoreCase)}{appConfigs.QidoStudyLevelUrlPathWithoutOrgId}";

            Report.Step(@"Browse endpoint url and get the cookie from browser", @"Should get the valid cookie");
            LoginToEndpoint(_apiGatewayBaseUrlWithOrgNamePlaceholder.Replace(placeHolderForOrgReplacement, tenantDetails[Constants.ValidTenantKey].TenantName, StringComparison.InvariantCultureIgnoreCase), pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            _cookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            _headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            AssertTest.IsTrue(_cookie != null, failMsg: "Cookie is not available", passMsg: $"Cookie is available, Cookie value: {_cookie.CookieValue}");
        }

        [TestCategory(nameof(TestCategory.OrgNameInUrl))]
        [TestMethod]
        public void APICallWithValidTenantAsSubDomainTest()
        {
            Report.Step(@"GET API call to the CDR service with Org Name(org1) as sub domain in Url", @"Should get the valid CDR service response");
            CDRGetCallAndAssert(_cdrSubscriptionUrlWithOrg1);
        }

        [TestCategory(nameof(TestCategory.OrgNameInUrl))]
        [TestMethod]
        public void APICallWithValidTenantAsSubDomainAndVerifyOrgIdInUpstreamUrlTest()
        {
            //pathMap= {["/mockserviceA/multitenancy"] = "/mockserviceA/multitenancy/orgId"} should be added in of global lua filter in envoy conifg

            Report.Step(@"API call to Less payload mockservice with Org Name (org1) and verify the OrgId in upstream url", @"The upstream url should have org1 orgId");
            var customUniqueRequestHeaderValue = Guid.NewGuid().ToString();
            _headers.Add(Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue);
            string baseUrl = _apiGatewayBaseUrlWithOrgNamePlaceholder.Replace(placeHolderForOrgReplacement, tenantDetails[Constants.ValidTenantKey].TenantName, StringComparison.InvariantCultureIgnoreCase);
            string multitenancyMockserviceUrl = $"{ baseUrl }{ appConfigs.MultitenancyMockservice}";
            Logger.Info($"Mockservice Url: {multitenancyMockserviceUrl}");
            _ = HttpClientUtility.ExecuteAsync(HttpMethod.Get, multitenancyMockserviceUrl, _headers, null);
            CommonBL commonBL = new CommonBL();
            _headers.Remove(Constants.CustomUniqueRequestHeaderName);
            var upstreamRequestUrl = commonBL.GetUpstreamRecordedRequest(baseUrl, _headers, Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue, "LessPayloadMockservice.json");
            string reqUrl = upstreamRequestUrl["path"].ToString();
            Logger.Info($"Upstream request url: { reqUrl}");
            AssertTest.IsTrue(upstreamRequestUrl != null && reqUrl.EndsWith(tenantDetails[Constants.ValidTenantKey].IamOrganizationId, StringComparison.InvariantCultureIgnoreCase),
                failMsg: $"OrgId is not found in upstream request headers for OrgName: {tenantDetails[Constants.ValidTenantKey].TenantName}", passMsg: $"OrgId is available in upstream request headers for the OrgName: {tenantDetails[Constants.ValidTenantKey].TenantName}");
        }

        [TestCategory(nameof(TestCategory.OrgNameInUrl))]
        [TestMethod]
        public void APICallWithInValidTenantAsSubDomainAndOrgNameNotInStaticConfigurationTest()
        {
            Report.Step(@"GET API call to the CDR service with invalid Org Name as sub domain which is not in static configuration", @"Should get the 404 error response");
            Logger.Info($"CDR service invalid org name Url: {_cdrSubscriptionUrlWithInvalidOrgName}");
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, _cdrSubscriptionUrlWithInvalidOrgName, _headers, null).Result;
            AssertTest.IsTrue(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound,
                 failMsg: "Failed to get 404 status code for the CDR service response", passMsg: "Received the 404 status code for the CDR service response");
        }

        [TestCategory(nameof(TestCategory.OrgNameInUrl))]
        [TestMethod]
        public void APICallWithInvalidTenantAsSubDomainAndInvalidOrgIdInHeaderTest()
        {
            Report.Step(@"GET API call to the CDR service with Org Name(org1) as sub domain in Url and invalid OrgId in headers", @"Should get the valid CDR service response");
            _headers.Add("edisp-org-id", tenantDetails[Constants.InvalidTenantKey].IamOrganizationId);
            Logger.Info($"Invalid OrgId: {tenantDetails[Constants.InvalidTenantKey].IamOrganizationId}");
            CDRGetCallAndAssert(_cdrSubscriptionUrlWithOrg1);
        }

        [TestCategory(nameof(TestCategory.OrgIdInHeader))]
        [TestMethod]
        public void APICallWithValidOrgIdInHeadersTest()
        {
            Report.Step(@"GET API call to the CDR service with valid OrgId in header", @"Should get the valid CDR service response");
            _headers.Add("edisp-org-id", tenantDetails[Constants.ValidTenantKey].IamOrganizationId);
            Logger.Info($"Valid OrgId: {tenantDetails[Constants.ValidTenantKey].TenantName}");
            CDRGetCallAndAssert(_cdrSubscriptionUrlWithOrg1);
        }

        [TestCategory(nameof(TestCategory.OrgIdInHeader))]
        [TestMethod]
        public void APICallWithInValidOrgIdInHeaderTest()
        {
            Report.Step(@"GET API call to the dicom service with invalid OrgId in header", @"Should get the 403 error response");
            _headers.Add("edisp-org-id", tenantDetails[Constants.InvalidTenantKey].IamOrganizationId);
            Logger.Info($"Qido study level Url: {_qidoStudyLevelUrlWithInvalidOrgName}");
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, _qidoStudyLevelUrlWithInvalidOrgName, _headers, null).Result;
            AssertTest.IsTrue(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Forbidden,
                 failMsg: "Failed to get 403 status code in response for the Qido call with invalid OrgId in header", passMsg: "Received the 403 status code response for the qido call with invalid Orgid in header ");
        }

        [TestCategory(nameof(TestCategory.OrgIdInHeader))]
        [TestMethod]
        public void APICallWithoutOrgIdInHeaderTest()
        {
            Report.Step(@"GET API call to the dicom service without OrgId in header", @"Should get the 404 error response");
            Logger.Info($"Qido study level Url: {_qidoStudyLevelUrlWithInvalidOrgName}");
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, _qidoStudyLevelUrlWithInvalidOrgName, _headers, null).Result;
            AssertTest.IsTrue(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound,
                 failMsg: "Failed to get 404 status code in response for the Qido call without OrgId in header", passMsg: "Received the 404 status code response for the qido call without Orgid in header ");
        }

        private void CDRGetCallAndAssert(string _crdSubscriptionUrl)
        {
            Logger.Info($"CDR subscription Url: {_crdSubscriptionUrl}");
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, _crdSubscriptionUrl, _headers, null);
            AssertTest.IsTrue(responseBody != null && responseBody.ContainsKey("resourceType") && responseBody["resourceType"].ToString().EqualsWithIgnoreCase("Bundle"),
                 failMsg: "Failed to get the CDR service success response", passMsg: "Received the CDR service success response");
        }
    }
}
