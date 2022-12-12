using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities.Wait;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.UITests.AuthenticationTests
{
    [TestClass]
    public class AuthenticationTests : BaseTest
    {
        private readonly CommonSteps _commonSteps = new CommonSteps();

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void BrowseEndpointAndLoginWithValidCredentialsTest()
        {
            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void BrowseEndpointAndLoginWithInvalidCredentialsTest()
        {
            Report.Step(@"Browse the endpoint url and login with invalid UserName", @"Login should be unsuccessfull");
            var isLoginSuccessful = LoginToEndpoint(defaultEndpointUrl, userName: "abc@philips.com", "abc123");
            AssertTest.IsFalse(isLoginSuccessful, failMsg: "Login successful with invalid credentials", passMsg: "Unsuccessful Login");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APICallWithValidCookieTest()
        {
            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);


            Report.Step(@"API call with cookie taken from browser", @"Should get the valid upstream response");
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            var cookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            AssertTest.IsTrue(cookie != null, failMsg: "Cookie is not available", passMsg: $"Cookie is available, Cookie value: {cookie.CookieValue}");
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, cookie.CookieValue);
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, headers, null);
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APICallInBrowserWithInValidCookieTest()
        {
            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);


            Report.Step(@"Delete/Modify the cookie in browser and browse again the same endpoint url", @"Should get a new cookie and that passes to upstream and get the valid upstream response");
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            var beforeCookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            beforeCookie.CookieValue = $"abc123{beforeCookie.CookieValue}";
            authenticationBL.SetCookie(beforeCookie);
            WebDriver.Goto(defaultEndpointUrl);
            var afterCookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            var responseBody = GetResponseFromUI();
            AssertTest.AreNotEqual(beforeCookie.CookieValue, afterCookie.CookieValue);
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received after cookie deleted/modified", passMsg: "Received Success response after cookie deleted/modified");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void AccessUpstreamJustBeforeSessionTimeoutTest()
        {
            //Make sure to set "OAUTH2_PROXY_COOKIE_REFRESH" = "0h0m15s" (15 seconds) in Oauth proxy service for automaiton

            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);


            Report.Step($"Wait till before session timeout ({pipelineConfigs.OauthProxyCookieTimeoutInSeconds} seconds)", @"Cookie should not change before the session timeout");
            int waitTimeInSeconds = -18;
            AssertTest.IsFalse(WaitForGivenTimeAndGetIsCookieChanged(waitTimeInSeconds), failMsg: $"Cookie has changed before the session timeout: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds} and wait time: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds + waitTimeInSeconds}",
                passMsg: $"Cookie has not changed before the session timeout: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds} and wait time: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds + waitTimeInSeconds}");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void AccessUpstreamAfterSessionTimeoutTest()
        {
            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);


            Report.Step($"Wait till after session timeout ({pipelineConfigs.OauthProxyCookieTimeoutInSeconds} seconds)", @"Cookie should change after the session timeout");
            int waitTimeInSeconds = 2;
            AssertTest.IsTrue(WaitForGivenTimeAndGetIsCookieChanged(waitTimeInSeconds), failMsg: $"Cookie has not changed after the session timeout: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds} and wait time: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds + waitTimeInSeconds}",
                passMsg: $"Cookie has changed after the session timeout: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds} and wait time: {pipelineConfigs.OauthProxyCookieTimeoutInSeconds + waitTimeInSeconds}");
        }

        private bool WaitForGivenTimeAndGetIsCookieChanged(int waitTimeInSeconds)
        {
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            var beforeCookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            int totalWaitTime = pipelineConfigs.OauthProxyCookieTimeoutInSeconds + waitTimeInSeconds;
            if (totalWaitTime > 0)
            {
                Sleep.Seconds(totalWaitTime);
            }
            WebDriver.Goto(defaultEndpointUrl);
            var afterCookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            AssertTest.IsTrue(true,failMsg:"",passMsg:$"Before cookie:{beforeCookie.CookieValue}\nAfterCookie:{afterCookie.CookieValue}");
            return !beforeCookie.CookieValue.Equals(afterCookie.CookieValue);
        }
    }
}
