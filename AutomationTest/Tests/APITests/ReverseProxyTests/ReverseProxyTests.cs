using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Driver.UI.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITest
{
    [TestClass]
    public class ReverseProxyTests : BaseTest
    {
        private Cookies _cookie;
        private readonly CommonSteps _commonSteps = new CommonSteps();
        private readonly string _heavyPayloadMockServiceUrl = $"{pipelineConfigs.APIGatewayBaseUrl}/{ appConfigs.HeavyPayloadMockservice}";
        private readonly string _serviceUnavailableMockServiceUrl = $"{pipelineConfigs.APIGatewayBaseUrl}/{ appConfigs.ServiceUnavailableMockservice}";

        [TestInitialize]
        public void BeforeTest()
        {
            Logger.Info("Before test");
            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);

            Report.Step(@"Get the cookie from browser", @"Should get the valid cookie");
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            _cookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            AssertTest.IsTrue(_cookie != null, failMsg: "Cookie is not available", passMsg: $"Cookie is available, Cookie value: {_cookie.CookieValue}");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void PostCallWithLessPayloadTest()
        {
            Report.Step(@"POST API call to the less payload mockservice with the cookie", @"Should get the valid upstream response with Status: Success");
            var (headers, requestBody) = GetRequestBody(defaultEndpointUrl);
            var postResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Post, defaultEndpointUrl, headers, requestBody);
            AssertTest.IsTrue(postResponseBody != null && postResponseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void PostCallWithHeavyPayloadTest()
        {
            Report.Step(@"POST API call to the Heavy payload mockservice with the cookie", @"Should get the valid upstream response with Status: Success");
            var (headers, requestBody) = GetRequestBody(_heavyPayloadMockServiceUrl);
            var postResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Post, _heavyPayloadMockServiceUrl, headers, requestBody);
            AssertTest.IsTrue(postResponseBody != null && postResponseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void PostCallWhenUpstreamServiceDownTest()
        {
            Report.Step(@"POST API call to the Service unavailable mockservice with the cookie", @"Should get 503 Service Unavailable status");
            var (headers, requestBody) = GetRequestBody(defaultEndpointUrl);
            var postResponseBody = HttpClientUtility.ExecuteAsync(HttpMethod.Post, _serviceUnavailableMockServiceUrl, headers, requestBody);
            AssertTest.IsTrue(postResponseBody.Result.StatusCode == HttpStatusCode.ServiceUnavailable, failMsg: "No StatusCode:503 Service Unavailable status received", passMsg: "Received StatusCode:503 Service Unavailable status");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void GetCallWithLessPayloadTest()
        {
            Report.Step(@"GET API call to the less payload mockservice with the cookie", @"Should get the valid upstream response with Status: Success");
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            Logger.Info($"GET endpoint Url: {defaultEndpointUrl}");
            var getResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, headers, null);
            AssertTest.IsTrue(getResponseBody != null && getResponseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void GetCallWithHeavyPayloadTest()
        {
            Report.Step(@"GET API call to the Heavy payload mockservice with the cookie", @"Should get the valid upstream response with Status: Success");
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            Logger.Info($"GET endpoint Url: {_heavyPayloadMockServiceUrl}");
            var getResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, _heavyPayloadMockServiceUrl, headers, null);
            AssertTest.IsTrue(getResponseBody != null && getResponseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void GetCallWhenUpstreamServiceDownTest()
        {
            Report.Step(@"GET API call to the Service unavailable mockservice with the cookie", @"Should get 503 Service Unavailable status");
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            Logger.Info($"GET endpoint Url: {_serviceUnavailableMockServiceUrl}");
            var getResponseBody = HttpClientUtility.ExecuteAsync(HttpMethod.Get, _serviceUnavailableMockServiceUrl, headers, null);
            AssertTest.IsTrue(getResponseBody.Result.StatusCode == HttpStatusCode.ServiceUnavailable, failMsg: "No StatusCode:503 Service Unavailable status received", passMsg: "Received StatusCode:503 Service Unavailable status");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void PutCallWithHeavyPayloadTest()
        {
            Report.Step(@"PUT API call to the Heavy payload mockservice with the cookie", @"Should get the valid upstream response with Status: Success");
            var (headers, requestBody) = GetRequestBody(defaultEndpointUrl);
            Logger.Info($"PUT call endpoint Url: {_heavyPayloadMockServiceUrl}");
            var putResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Put, _heavyPayloadMockServiceUrl, headers, requestBody);
            AssertTest.IsTrue(putResponseBody != null && putResponseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void PutCallWhenUpstreamServiceDownTest()
        {
            Report.Step(@"PUT API call to the Service unavailable mockservice with the cookie", @"Should get 503 Service Unavailable status");
            var (headers, requestBody) = GetRequestBody(defaultEndpointUrl);
            Logger.Info($"PUT call endpoint Url: {_serviceUnavailableMockServiceUrl}");
            var putResponseBody = HttpClientUtility.ExecuteAsync(HttpMethod.Put, _serviceUnavailableMockServiceUrl, headers, requestBody);
            AssertTest.IsTrue(putResponseBody.Result.StatusCode == HttpStatusCode.ServiceUnavailable, failMsg: "No StatusCode:503 Service Unavailable status received", passMsg: "Received StatusCode:503 Service Unavailable status");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void DeleteCallTest()
        {
            Report.Step(@"DELETE API call to the Heavy payload mockservice with cookie", @"Should get the valid upstream response with Status: Success");
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            Logger.Info($"DELETE call endpoint Url: {_heavyPayloadMockServiceUrl}");
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Delete, _heavyPayloadMockServiceUrl, headers, null);
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void DeleteCallWhenUpstreamServiceDownTest()
        {
            Report.Step(@"DELETE API call to the Service unavailable mockservice with cookie", @"Should get 503 Service Unavailable status");
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            Logger.Info($"DELETE call endpoint Url: {_serviceUnavailableMockServiceUrl}");
            var responseBody = HttpClientUtility.ExecuteAsync(HttpMethod.Delete, _serviceUnavailableMockServiceUrl, headers, null);
            AssertTest.IsTrue(responseBody.Result.StatusCode == HttpStatusCode.ServiceUnavailable, failMsg: "No StatusCode:503 Service Unavailable status received", passMsg: "Received StatusCode:503 Service Unavailable status");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void CDRUpstreamServiceAPICallTest()
        {
            Report.Step(@"GET API call to the CDR service with cookie", @"Should get the valid CDR service response");
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            string cdrServiceUrl = $"{pipelineConfigs.APIGatewayBaseUrl}{appConfigs.CDRImagingStudyUrlPath.Replace("OrgId", tenantDetails[Constants.ValidTenantKey].IamOrganizationId)}";
            Logger.Info($"CDR service Url: {cdrServiceUrl}");
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, cdrServiceUrl, headers, null);
            AssertTest.IsTrue(responseBody != null && responseBody.ContainsKey("resourceType") && responseBody["resourceType"].ToString().EqualsWithIgnoreCase("Bundle"),
                 failMsg: "Failed to get the CDR service success response", passMsg: "Received the CDR service success response");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APICallsFromUIUpstreamInBrowserTest()
        {
            Report.Step(@"Browse the mountebank imposters endpoint url", @"Should get the mountebank imposters UI page in browser");
            var impostersUrl = $"{pipelineConfigs.APIGatewayBaseUrl}/imposters";
            Logger.Info($"Imposters page Url: {impostersUrl}");
            ReverseProxyBL reverseProxyBL = new ReverseProxyBL(WebDriver);
            AssertTest.IsTrue(reverseProxyBL.NavigateToImpostersPageAndGetIsMockServiceLinksDisplayed(impostersUrl), failMsg: "Imposters page is not displayed", passMsg: "Imposters page is displayed");


            Report.Step(@"Click on less payload mockservice link and verify", @"Should get the less payload mockservice contents");
            var isMockserviceADisplayed = reverseProxyBL.ClickOnImposterAndGetMockServiceContentIsDisplayed("ServiceA");
            AssertTest.IsTrue(isMockserviceADisplayed, failMsg: "Less payload mockservice content is not displayed", passMsg: "Less payload mockservice content is displayed");


            Report.Step(@"Click on Heavy payload mockservice link and verify", @"Should get the Heavy payload mockservice contents");
            WebDriver.ClickOnBrowserBackButton();
            var isMockserviceBDisplayed = reverseProxyBL.ClickOnImposterAndGetMockServiceContentIsDisplayed("ServiceB");
            AssertTest.IsTrue(isMockserviceBDisplayed, failMsg: "Heavy payload mockservice content is not displayed", passMsg: "Heavy payload mockservice content is displayed");
        }

        private (Dictionary<string, string>, StringContent) GetRequestBody(string endpointUrl)
        {
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, _cookie.CookieValue);
            Logger.Info($"GET API call endpoint Url: {endpointUrl}");
            var getResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, headers, null);
            var requestBody = HttpClientUtility.CreateHttpContent(getResponseBody.ToString());
            Logger.Info($"Request body: {requestBody}");
            return (headers, requestBody);
        }
    }
}
