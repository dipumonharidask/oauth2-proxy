using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using System;
using System.Collections.Generic;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using System.Net.Http;
using Reporters;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Wait;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITests.SSOTokenAuthenticatorTests
{
    [TestClass]
    public class SSOTokenAuthenticatorTests : BaseTest
    {
        private readonly VueSSOTokenBL _ssoTokenBL = new VueSSOTokenBL();
        private readonly HSPIAMBusinessLayer _iamBL = new HSPIAMBusinessLayer();
        private string ssoToken;
        private string userName;
        string cdrServiceUrl = $"{pipelineConfigs.APIGatewayBaseUrl}{appConfigs.CDRImagingStudyUrlPath.Replace("OrgId", tenantDetails[Constants.ValidTenantKey].IamOrganizationId)}";
        string getStudyDetailsUrl = $"{pipelineConfigs.CDRBaseUrl}{appConfigs.CDRImagingStudyUrlPath.Replace("OrgId", tenantDetails[Constants.ValidTenantKey].IamOrganizationId)}?_count=1";

        [TestCategory(nameof(TestCategory.SSOToken))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void UserwithPermissionsTest()
        {
            Report.Step(@"CDR Call with SSO token header of user having required permission", @"Should get the valid upstream response");
            var response = GetUpstreamResponseWithSSOToken(pipelineConfigs.SSOTokenUserName, pipelineConfigs.OrgSymmetricKey);
            AssertTest.IsTrue(response.ToString().Contains("StatusCode: 200"), failMsg: $"StatusCode was not 200, Resopnse Message: {response}", passMsg: "Received success response with statuscode as 200");
        }

        [TestCategory(nameof(TestCategory.SSOToken))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void CreateUserwithoutPermissionsTest()
        {
            try
            {
                Report.Step(@"CDR Call with SSO token header of a new user not having required permission", @"Should get 401 unauthorized upstream response");
                Random rand = new Random();
                userName = "autoTest"+ rand.Next(99, 9999);
                var response = GetUpstreamResponseWithSSOToken(userName, pipelineConfigs.OrgSymmetricKey);
                AssertTest.IsTrue(response.ToString().Contains("StatusCode: 401"), failMsg: "CDR call did not returned Unauthorized access",
                    passMsg: "CDR call returned Status code of 401 Unauthorized access as expected");
            }
            catch (Exception e)
            {
                Report.ReportError("Exception",e.ToString());
                
            }
            finally
            {
                //Deleting the created automation user
                var userAccessTokenHeader = CreateUserAccessTokenHeader();
                _ = _iamBL.DeleteUser(pipelineConfigs.IAMGetUserUrl, pipelineConfigs.IDMClientBaseUrl, userName+ "_rubyhealthiamte@vue.com", userAccessTokenHeader);
            }
        }

        [TestCategory(nameof(TestCategory.SSOToken))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void UserwithWrongSSOTokenHeaderTest()
        {
            Report.Step(@"CDR Call with with wrong SSO token header of a user", @"Should get invalid sso token upstream response");
            var headers = new Dictionary<string, string>();
            headers.TryAdd("EDISP-vuesso", "InvalidSSOToken");
            headers.TryAdd("edisp-vuesso-orgid", tenantDetails[Constants.ValidTenantKey].IamOrganizationId);
            var response = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, cdrServiceUrl, headers, null);
            AssertTest.IsTrue(response["Detail"].ToString().EqualsWithIgnoreCase("Please provide valid sso token and symmetric key"), 
                failMsg: "CDR call did not returned 'Please provide valid sso token and symmetric key' error response",
               passMsg: "CDR call returned error message 'Please provide valid sso token and symmetric key' as expected");
        }

        [TestCategory(nameof(TestCategory.SSOToken))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void UserwithInvalidOrgIdHeaderTest()
        {
            Report.Step(@"CDR Call with with wrong orgId header", @"Should get the given key is not present in the dictionary upstream response");
            var headers = new Dictionary<string, string>();
            ssoToken = _ssoTokenBL.GenerateSsoToken(pipelineConfigs.SSOTokenUserName, DateTime.UtcNow.AddMinutes(0).ToString(Constants.TimeStampFormat), pipelineConfigs.OrgSymmetricKey);
            headers.TryAdd("EDISP-vuesso", ssoToken);
            headers.TryAdd("edisp-vuesso-orgid", string.Empty);
            var response = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, cdrServiceUrl, headers, null);
            var message = "orgId is empty, set it in edisp-vuesso-orgid header";
            AssertTest.IsTrue(response["Detail"].ToString().EqualsWithIgnoreCase(message),
                failMsg: $"CDR call did not returned '{message}' error response",
                passMsg: $"CDR call returned error message '{message}' as expected");
        }

        [TestCategory(nameof(TestCategory.SSOToken))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void UserwithExpiredSSOTokenTest()
        {
            Report.Step(@"CDR Call with with Expired SSO token header of a user with all required permission", @"Should get invalid sso token session timed out upstream response");
            ssoToken = _ssoTokenBL.GenerateSsoToken(pipelineConfigs.SSOTokenUserName, DateTime.UtcNow.AddMinutes(-31).ToString(Constants.TimeStampFormat), pipelineConfigs.OrgSymmetricKey);
            var headers = new Dictionary<string, string>();
            headers.TryAdd("EDISP-vuesso", ssoToken);
            headers.TryAdd("edisp-vuesso-orgid", tenantDetails[Constants.ValidTenantKey].IamOrganizationId);
            var response = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, cdrServiceUrl, headers, null);
            AssertTest.IsTrue(response["Detail"].ToString().EqualsWithIgnoreCase("SSO token session timed out"),
               failMsg: "CDR call did not returned 'SSO token session timed out' error response",
              passMsg: "CDR call returned error message 'SSO token session timed out' as expected");
        }

        [TestCategory(nameof(TestCategory.SSOToken))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public async Task AccessTokenValueisTakenFromCacheTest()
        {
            Report.Step(@"Verify that new session with same user uses same cached accesstoken",
                @"same cached access token should be used");
            List<string> accessTokenValues = new List<string>();

            for (int i = 0; i < 2; i++)
            {
                var headers = new Dictionary<string, string>();
                string customUniqueRequestHeaderValue = Guid.NewGuid().ToString();
                headers.Add(Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue);
                ssoToken = _ssoTokenBL.GenerateSsoToken(pipelineConfigs.SSOTokenUserName, DateTime.UtcNow.ToString(Constants.TimeStampFormat), pipelineConfigs.OrgSymmetricKey);

                headers.TryAdd("EDISP-vuesso", ssoToken);
                headers.TryAdd("edisp-vuesso-orgid", tenantDetails[Constants.ValidTenantKey].IamOrganizationId);
                Sleep.Seconds(appConfigs.MountibankTimeoutinSeconds);

                _ = await HttpClientUtility.ExecuteAsync(HttpMethod.Get, defaultEndpointUrl, headers, null);
                CommonBL commonBL = new CommonBL();
                var testHeaderRequest = commonBL.GetUpstreamRecordedRequest(pipelineConfigs.APIGatewayBaseUrl, headers, Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue, "LessPayloadMockservice.json");
                accessTokenValues.Add(testHeaderRequest["headers"]["authorization"].ToString());
            }
            AssertTest.IsTrue(accessTokenValues.First().Equals(accessTokenValues.Last()), failMsg: "New session with same user is not using same cached accesstoken",
                passMsg: "New session with same user is using cached accesstoken");
        }

        [TestCategory(nameof(TestCategory.SSOToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestMethod]
        public void InvalidSymmetricKeyTest()
        {
            Report.Step(@"CDR Call with SSO token header of Invalid Symmetric Key", @"Should get 401 unauthorized upstream response");
            var response = GetUpstreamResponseWithSSOToken(pipelineConfigs.SSOTokenUserName, "rpJupVvvHiX5kgrPllV8gWsurbzSu9D99kUwamFdL9I=");
            AssertTest.IsTrue(response.ToString().Contains("StatusCode: 401"), failMsg: "CDR call did not returned Unauthorized access",
                passMsg: "CDR call returned Status code of 401 Unauthorized access as expected");
        }

        /// <summary>
        /// Method to get the Upstream response with SSO token
        /// </summary>
        /// <param name="userName">UserName to generate the SSO Token</param>
        /// <param name="addMin">addMinutes value</param>
        /// /// <param name="symmetricKey">SymmetricKey of org value</param>
        /// <returns>HttpResponseMessage</returns>
        private HttpResponseMessage GetUpstreamResponseWithSSOToken(String userName,string symmetricKey, int addMin = 0)
        {
            try
            {
                Report.Step(@"CDR Call with SSO token header of user having required permission", @"Should get the valid upstream response");
                ssoToken = _ssoTokenBL.GenerateSsoToken(userName, DateTime.UtcNow.AddMinutes(addMin).ToString(Constants.TimeStampFormat), symmetricKey);
                var headers = new Dictionary<string, string>();
                Sleep.Seconds(appConfigs.MountibankTimeoutinSeconds);
                headers.TryAdd("edisp-vuesso", ssoToken);
                headers.TryAdd("edisp-vuesso-orgid", tenantDetails[Constants.ValidTenantKey].IamOrganizationId);

                return HttpClientUtility.ExecuteAsync(HttpMethod.Get, cdrServiceUrl + "/" + GetStudyUidDetails(), headers, null).Result;
            }
            catch (Exception e)
            {
                Report.ReportError("Exception", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Method to get the single studyuid details based on organization
        /// </summary>
        /// <returns>string of study id</returns>
        private string GetStudyUidDetails()
        {
            Report.Step(@"Get Study details for the Org", @"Should get the study details");
            var headers = CreateUserAccessTokenHeader();
            headers.TryAdd("api-version", "1");
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, getStudyDetailsUrl, headers, null);
            var studyDetails = responseBody["entry"]?.Where(x => x["resource"]["id"] != null).Select(x => x["resource"]?["id"]?.ToString()).ToList()[0];

            if (string.IsNullOrEmpty((studyDetails)))
            {
                AssertTest.IsTrue(false, "Fetched Study details were empty", "", false);
            }
            else
            {
                AssertTest.IsTrue(true, "", "Study details were fetched", false);
            }

            return studyDetails;
        }

        /// <summary>
        /// Method to created Access Token header
        /// </summary>
        /// <returns>Dictionary<Key, value></returns>
        private Dictionary<string, string> CreateUserAccessTokenHeader()
        {
            var userAccessTokenHeader = HttpClientUtility.CreateUserAccessTokenHeader(pipelineConfigs);
            AssertTest.IsTrue(userAccessTokenHeader != null, failMsg: "User Access Token header is null", passMsg: $"User Access Token header is not null");
            return userAccessTokenHeader;
        }
    }
}
