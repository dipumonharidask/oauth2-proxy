using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Driver.UI.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITests.RequestTransformationHeadersTests
{
    [TestClass]
    public class RequestTransformationHeadersTests : BaseTest
    {
        private Cookies _cookie;
        private Dictionary<string, string> _headers;


        [TestInitialize]
        public void BeforeTest()
        {
            Logger.Info("Before test");
            CommonSteps _commonSteps = new CommonSteps();
            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);

            Report.Step(@"Get the cookie from browser", @"Should get the valid cookie");
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            _cookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            _headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            AssertTest.IsTrue(_cookie != null, failMsg: "Cookie is not available", passMsg: $"Cookie is available, Cookie value: {_cookie.CookieValue}");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void CustomHeaderAddedToUpstreamRequestByAPIGatewayTest()
        {
            //custom-header:"100" should be added to less payload mockserviceA in envoy config

            Report.Step(@"API call to Less payload mockservice and make an imposters api call with mockservice port number to get the recorded upstream request from API Gateway",
                @"custom-header should be available in the upstream request headers");
            Logger.Info($"Mockservice Url: {defaultEndpointUrl}");
            _ = HttpClientUtility.ExecuteAsync(HttpMethod.Get, defaultEndpointUrl, _headers, null);
            CommonBL commonBL = new CommonBL();
            var getResponseBody = commonBL.GetImposterRecordedRequests(pipelineConfigs.APIGatewayBaseUrl, _headers, "LessPayloadMockservice.json");
            var isRecordRequest = (bool)getResponseBody["recordRequests"];
            AssertTest.IsTrue(isRecordRequest, failMsg: "Record requests is not enabled in Less payload mockservice imposter", passMsg: "Record request is enabled in Less payload mockservice imposter");
            var customHeaderRequest = getResponseBody["requests"].Where(x => x["headers"]["custom-header"] != null && x["headers"]["custom-header"].ToString() == "100").FirstOrDefault();

            AssertTest.IsTrue(customHeaderRequest != null, failMsg: "custom-header is not found in upstream request headers", passMsg: "custom-header is available in upstream request headers");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void PrefixReWriteTest()
        {
            //Make sure envoy config should have prefix:"/prefixrewritetest" and prefix_rewrite:"/mockserviceA"

            Report.Step(@"API call with /prefixrewritetest/test url path", @"Url should be updated automatically to /mockserviceA/test and get the upstream response");
            var customUniqueRequestHeaderValue = Guid.NewGuid().ToString();
            _headers.Add(Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue);
            string prefixReWriteMockserviceUrl = $"{ pipelineConfigs.APIGatewayBaseUrl}/prefixrewritetest/test";
            Logger.Info($"Mockservice Url: {prefixReWriteMockserviceUrl}");
            var upstreamResponse = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, prefixReWriteMockserviceUrl, _headers, null);
            CommonBL commonBL = new CommonBL();
            _headers.Remove(Constants.CustomUniqueRequestHeaderName);
            var upstreamRequestUrl = commonBL.GetUpstreamRecordedRequest(pipelineConfigs.APIGatewayBaseUrl, _headers, Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue, "LessPayloadMockservice.json");
            string reqUrl = upstreamRequestUrl["path"].ToString();
            Logger.Info($"Upstream request url: { reqUrl}");
            AssertTest.IsTrue(upstreamRequestUrl != null && reqUrl.EndsWith(appConfigs.LessPayloadMockservice, StringComparison.InvariantCultureIgnoreCase), failMsg: "custom-header is not found in upstream request headers", passMsg: "custom-header is available in upstream request headers");
            AssertTest.IsTrue(upstreamResponse != null && upstreamResponse["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }
    }
}
