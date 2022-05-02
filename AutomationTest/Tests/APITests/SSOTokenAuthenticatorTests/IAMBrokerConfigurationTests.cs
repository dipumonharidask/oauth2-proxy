using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using TimeZoneConverter;
using Utilities;
using Utilities.Wait;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITests
{
    [TestClass]
    public class IAMBrokerConfigurationTests : BaseTest
    {
        private readonly string _setKeyUrl = $"{pipelineConfigs.APIGatewayBaseUrl}{appConfigs.IamBrokerConfigAPIRelativePath.Replace("OrgId", tenantDetails[Constants.ValidTenantKey].IamOrganizationId)}";
        private readonly VueSSOTokenBL _ssoTokenBL = new VueSSOTokenBL();

        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.IAMTokenExchangeBrokerPreCondition))]
        [TestMethod]
        public async Task SetSymmetricKeyWithValidAccessTokenTest()
        {
            Report.Step(@"Set symmetrickey with valid access token", @"Should get 204 NoContent response");
            var userAccessTokenHeader = HttpClientUtility.CreateUserAccessTokenHeader(pipelineConfigs);
            string url = $"{_setKeyUrl}{Constants.SymmetricKeyName}";
            Logger.Info($"Symmetrickey set url: {url}");
            var content = HttpClientUtility.CreateHttpContent($"\"{pipelineConfigs.OrgSymmetricKey}\"");
            var response = await HttpClientUtility.ExecuteAsync(HttpMethod.Post, url, userAccessTokenHeader, content);
            AssertTest.IsTrue(response != null && response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent, failMsg: $"Failed to set the symmetric key, StatusCode: {response.StatusCode}", passMsg: "Symmetrickey set successfully");
        }

        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.IAMTokenExchangeBroker))]
        [TestMethod]
        public async Task SetSymmetricKeyWithInvalidAccessTokenTest()
        {
            Report.Step(@"Set symmetrickey with invalid access token", @"Should get 401 Unauthorized response");
            var invalidAccessTokenHeader = new Dictionary<string, string>();
            invalidAccessTokenHeader.Add("Authorization", $"Bearer {Guid.NewGuid()}");
            string url = $"{_setKeyUrl}{Constants.SymmetricKeyName}";
            Logger.Info($"Symmetrickey set url: {url}");
            var content = HttpClientUtility.CreateHttpContent($"\"{pipelineConfigs.OrgSymmetricKey}\"");
            var response = await HttpClientUtility.ExecuteAsync(HttpMethod.Post, _setKeyUrl, invalidAccessTokenHeader, content);
            AssertTest.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized, failMsg: $"Not received HttpStatusCode 401 Unauthorized, Actual: {response.StatusCode}", passMsg: "Received HttpStatusCode.Unauthorized");
        }

        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.IAMTokenExchangeBroker))]
        [TestMethod]
        public async Task SetTimeZoneWithTokyoStandardTimeTest()
        {
            try
            {
                await SetTimeZoneAndValidate(Constants.TokyoTimeZone);
                var response = await APICallWithSSOTokenAndGetResponse(Constants.TokyoTimeZone, defaultEndpointUrl);
                AssertTest.IsTrue(response != null && response.IsSuccessStatusCode, failMsg: $"TimeZone:{Constants.TokyoTimeZone}, Not received HttpStatusCode 200, Actual: {response.StatusCode}", passMsg: "Received HttpStatusCode 200");

                response = await APICallWithSSOTokenAndGetResponse(Constants.UTCTimeZone, defaultEndpointUrl);
                AssertTest.IsTrue(response != null && response.StatusCode == HttpStatusCode.Unauthorized, failMsg: $"TimeZone:{Constants.UTCTimeZone}, Not received HttpStatusCode 401 Unauthorized, Actual: {response.StatusCode}", passMsg: "Received HttpStatusCode.Unauthorized");
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
            }
            finally
            {
                await SetTimeZoneAndValidate(Constants.UTCTimeZone);
                var response = await APICallWithSSOTokenAndGetResponse(Constants.UTCTimeZone, defaultEndpointUrl);
                AssertTest.IsTrue(response != null && response.IsSuccessStatusCode, failMsg: $"TimeZone:{Constants.UTCTimeZone}, Not received HttpStatusCode 200, Actual: {response.StatusCode}", passMsg: "Received HttpStatusCode 200");
            }
        }

        #region private methods

        private async Task SetTimeZoneAndValidate(string timeZoneName)
        {
            Report.Step($"Set {timeZoneName} timezone", @"Should get 204 NoContent response");
            var userAccessTokenHeader = HttpClientUtility.CreateUserAccessTokenHeader(pipelineConfigs);
            string url = $"{_setKeyUrl}{Constants.TimeZoneKeyName}";
            timeZoneName = $"\"{timeZoneName}\"";
            Logger.Info($"Set Timezone url: {url}");
            var content = HttpClientUtility.CreateHttpContent(timeZoneName);
            var response = await HttpClientUtility.ExecuteAsync(HttpMethod.Post, url, userAccessTokenHeader, content);
            AssertTest.IsTrue(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent, failMsg: $"Failed to set the time zone- {timeZoneName}, StatusCode: {response.StatusCode}", passMsg: $"TimeZone was set successfully, StatusCode: {response.StatusCode}");
        }

        private Task<HttpResponseMessage> APICallWithSSOTokenAndGetResponse(string timeZoneName, string url)
        {
            Report.Step($"API call with SSO token generated with {timeZoneName} timezone", @"Should get the upstream response");
            var tzi = TZConvert.GetTimeZoneInfo(timeZoneName);
            string ssoToken = _ssoTokenBL.GenerateSsoToken("user", TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi).DateTime.ToString(Constants.TimeStampFormat), pipelineConfigs.OrgSymmetricKey);
            Sleep.Seconds(appConfigs.MountibankTimeoutinSeconds);
            var headers = new Dictionary<string, string>();
            headers.TryAdd("EDISP-vuesso", ssoToken);
            headers.TryAdd("edisp-vuesso-orgid", tenantDetails[Constants.ValidTenantKey].IamOrganizationId);
            return HttpClientUtility.ExecuteAsync(HttpMethod.Get, url, headers, null);
        }

        #endregion
    }
}
