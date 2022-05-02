using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Driver.UI.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITests.IntrospectionTests
{
    [TestClass]
    public class IntrospectionTests : BaseTest
    {
        private Cookies _cookie;
        private Dictionary<string, string> _headers;

        [TestInitialize]
        public void BeforeTest()
        {
            Logger.Info("Before test");
            CommonSteps commonSteps = new CommonSteps();
            commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);

            Report.Step(@"Get the cookie from browser", @"Should get the valid cookie");
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            _cookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            _headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue); ;
            AssertTest.IsTrue(_cookie != null, failMsg: "Cookie is not available", passMsg: $"Cookie is available, Cookie value: {_cookie.CookieValue}");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void IntrospectValueToUpstreamRequestHeaderTest()
        {
            Report.Step(@"API call to Less payload mockservice and make an imposters api call with mockservice port number to get the recorded upstream request headers from API Gateway",
                @"edisp-introspect-value should be available in the upstream request headers");
            string customUniqueRequestHeaderValue = Guid.NewGuid().ToString();
            _headers.Add(Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue);
            Logger.Info($"Mockservice Url: { defaultEndpointUrl}");
            _ = HttpClientUtility.ExecuteAsync(HttpMethod.Get, defaultEndpointUrl, _headers, null);
            CommonBL commonBL = new CommonBL();
            var getResponseBody = commonBL.GetImposterRecordedRequests(pipelineConfigs.APIGatewayBaseUrl, _headers, "LessPayloadMockservice.json");
            var isRecordRequest = (bool)getResponseBody["recordRequests"];
            AssertTest.IsTrue(isRecordRequest, failMsg: "Record requests is not enabled in Less payload mockservice imposter", passMsg: "Record request is enabled in Less payload mockservice imposter");
            var testHeaderRequest = getResponseBody["requests"].Where(x => x["headers"][Constants.CustomUniqueRequestHeaderName] != null && x["headers"][Constants.CustomUniqueRequestHeaderName].ToString() == customUniqueRequestHeaderValue).FirstOrDefault();
            var testHeaderValue = testHeaderRequest["headers"][Constants.CustomUniqueRequestHeaderName].ToString();
            var introspectValue = testHeaderRequest["headers"]["edisp-introspect-value"].ToString();
            AssertTest.IsTrue(!string.IsNullOrWhiteSpace(testHeaderValue) && !string.IsNullOrWhiteSpace(introspectValue), failMsg: "edisp-introspect-value is not found in upstream request headers", passMsg: "edisp-introspect-value is available in upstream request headers");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void IntrospectValueFromUpstreamResponseHeaderTest()
        {
            Report.Step(@"GET API call to the CDR service with cookie",
                @"edisp-introspect-value should be available in the upstream response headers and Authorization should not be available in the upstream response header");
            string cdrServiceUrl = $"{pipelineConfigs.APIGatewayBaseUrl}{appConfigs.CDRSubscriptionUrlPath.Replace("OrgId", tenantDetails[Constants.ValidTenantKey].TenantName)}";
            Logger.Info($"CDR service Url: {cdrServiceUrl}");
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, cdrServiceUrl, _headers, null).Result;
            AssertTest.IsTrue(response != null && response.Headers.Contains("edisp-introspect-value") && !string.IsNullOrWhiteSpace(response.Headers.GetValues("edisp-introspect-value").FirstOrDefault()),
                 failMsg: "edisp-introspect-value is not found in upstream response headers", passMsg: "edisp-introspect-value is available in upstream request headers");
            AssertTest.IsTrue(response != null && !response.Headers.Contains("Authorization"),
                failMsg: "'Authorization' header found in upstream response headers", passMsg: "Authorization header is not available in upstream response headers");
        }
    }
}
