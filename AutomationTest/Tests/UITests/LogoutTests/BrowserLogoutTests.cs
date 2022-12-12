using Driver.UI.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using System.Net.Http;
using System.Threading.Tasks;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.UITests.LogoutTests
{
    [TestClass]
    public class BrowserLogoutTests : BaseTest
    {
        #region Tests
        CommonSteps _commonSteps = new CommonSteps();
        [TestInitialize]
        public void BeforeTest()
        {
            Report.Step(@"Login and open default endpoint url", @"should open the default mockservice page");
            
            _commonSteps.BrowseEndpointAndVerifyLogin(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.BrowserLogout))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void LogoutSuccessTest()
        {
            Report.Step(@"Call Logout api", @"should be redirected to the login page");
            AssertLogoutSuccess();
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.BrowserLogout))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public async Task ApiRequestBeforeAndAfterLogoutTest()
        {
            var cookie = FetchAndAssertCookieFromBrowser();
            var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, cookie.CookieValue);

            Report.Step(@"API call with cookie taken from browser", @"Should get the valid upstream response");
            var responseBeforeLogout = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, headers, null);
            AssertTest.IsTrue(responseBeforeLogout != null && responseBeforeLogout["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");

            Report.Step(@"Call Logout api", @"should be redirected to the login page");
            AssertLogoutSuccess();

            Report.Step(@"API call with older cookie taken from browser before logout", @"Should receive the HTML response of the IAM Login page");
            var responseAfterLogout = await HttpClientUtility.ExecuteAsync(HttpMethod.Get, defaultEndpointUrl, headers, null);
            var body = await responseAfterLogout?.Content?.ReadAsStringAsync();
            //Assuming login page is HSDP IAM's Login page
            AssertTest.IsTrue(responseAfterLogout != null && responseAfterLogout.IsSuccessStatusCode && body != null && body.Contains("<title>Philips</title>"), failMsg: "IAM login page's title is not displayed", passMsg: "IAM login page's title is displayed");

        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.BrowserLogout))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void BrowserRequestBeforeandAfterLogoutTest()
        {
            Report.Step(@"Browser request to the same endpoint", @"Should get the upstream response and login page should not be displayed");
            WebDriver.Goto(defaultEndpointUrl);
            Assert.IsFalse(IsLoginPageDisplayed());
            var responseBody = GetResponseFromUI();
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received Success response");


            Report.Step(@"Call Logout api", @"should be redirected to the login page");
            AssertLogoutSuccess();

            Report.Step(@"Browser request to the default endpoint url", @"Should receive IAM Login page");
            WebDriver.Goto(defaultEndpointUrl);
            //Assuming login page is HSDP IAM's Login page
            AssertTest.IsTrue(IsLoginPageDisplayed(),
                failMsg: "Not redirected back to login page", passMsg: "Redirected to login page");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.BrowserLogout))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void LogoutAfterTheSessionTerminatedTest()
        {
            Report.Step(@"Call Logout api", @"should be redirected to the login page");
            AssertLogoutSuccess();
            Report.Step(@"Calling Logout api again", @"should be redirected to the login page");
            //If we don't specify any redirect-uri it will give 422 Http response code
            AssertLogoutSuccess();
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.BrowserLogout))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void POST_HTTP_CallforLogoutAPITest()
        {
            Report.Step(@"Perform a POST Http call for Logout API", @"should be redirected to the login page");
            AssertTest.IsTrue(_commonSteps.LogoutWithPOST(defaultEndpointUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword),"Failed to Logout using PostCall",
                "Successfully logged out and redirected to login page with POST call");
        }


        #endregion Tests

        #region Private Methods
        private static Cookies FetchAndAssertCookieFromBrowser()
        {
            Report.Step(@"Get cookie from browser", @"Should get cookie from the current page");
            var authenticationBL = new AuthenticationBL(WebDriver);
            var cookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            AssertTest.IsTrue(cookie != null, failMsg: "Cookie is not available", passMsg: $"Cookie is available, Cookie value: {cookie.CookieValue}");
            return cookie;
        }

        private void AssertLogoutSuccess()
        {
            string GatewayLogoutEndpoint = $"{pipelineConfigs.APIGatewayBaseUrl}{appConfigs.LogoutPath}";
            WebDriver.Goto(GatewayLogoutEndpoint);
            AssertTest.IsTrue(IsLoginPageDisplayed(),
                failMsg: "Logout unsuccessful, not redirected back to login page", passMsg: "Successfully logged out and redirected to login page");
        }
        #endregion
    }
}
