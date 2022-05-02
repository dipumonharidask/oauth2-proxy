using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.CFUtilites;
using Philips.EDI.Foundation.APIGateway.AutomationTest.CFUtilites.Models;
using Reporters;
using Utilities;
using Utilities.Wait;
using System.Threading.Tasks;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITests.AccessTokenAuthenticationTests
{
    [TestClass]
    public class AccessTokenAuthenticationTests : BaseTest
    {
        private readonly CFUtility _cfUtility = new CFUtility(pipelineConfigs.CFBaseUrl, pipelineConfigs.CFUserName, pipelineConfigs.CFPassword, pipelineConfigs.CFOauthTokenUrl);
        private readonly HSPIAMBusinessLayer _iamBL = new HSPIAMBusinessLayer();

        private const string _permission = "BASIC.READ";
        private static string _permissionsJson = "{\"permissions\":[\"PermissionPlaceHolder\"]}".Replace("PermissionPlaceHolder", _permission);

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APIGetCallWithUserAccessTokenTest()
        {
            var userAccessTokenHeader = CreateUserAccessTokenHeader();
            APIGetCallWithAccessToken(userAccessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APICallsWithMultipleUserAccessTokenAndVerifySpecificAccessTokenIntrospectValueTest()
        {
            var userAccessTokenHeader1 = CreateUserAccessTokenHeader();
            var userAccessTokenHeader2 = CreateUserAccessTokenHeader();
            var userAccessTokenHeader3 = CreateUserAccessTokenHeader();
            var userAccessTokenHeader4 = CreateUserAccessTokenHeader();
            var userAccessTokenHeader5 = CreateUserAccessTokenHeader();

            APICallsWithMultipleAccessTokenAndVerifySpecificAccessTokenIntrospectValue(userAccessTokenHeader1, userAccessTokenHeader2, userAccessTokenHeader3, userAccessTokenHeader4, userAccessTokenHeader5);
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void CRUDApiCallWithUserAccessTokenTest()
        {
            var userAccessTokenHeader = CreateUserAccessTokenHeader();
            CRUDApiCallWithAccessToken(userAccessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APIGetCallWithInvalidAccessTokenTest()
        {
            Report.Step(@"API call with access token", @"Should get the 401 Unauthorized response");
            var invalidAccessTokenHeader = new Dictionary<string, string>();
            invalidAccessTokenHeader.Add("Authorization", $"Bearer {Guid.NewGuid().ToString()}");
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, defaultEndpointUrl, invalidAccessTokenHeader, null).Result;
            AssertTest.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized, failMsg: "Not received HttpStatusCode.Unauthorized", passMsg: "Received HttpStatusCode.Unauthorized");
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APIGetCallWithRefreshTokenTest()
        {
            Report.Step(@"API call with refresh token", @"Should get the 401 Unauthorized response");
            var userAccessTokenHeader = CreateUserAccessTokenHeader("refresh_token");
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, defaultEndpointUrl, userAccessTokenHeader, null).Result;
            AssertTest.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized, failMsg: "Not received HttpStatusCode.Unauthorized", passMsg: "Received HttpStatusCode.Unauthorized");
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void ChangePermissionInRoleAndVerifyUserAccessTokenPermissionsBeforeRedisCacheTimeoutTest()
        {
            var userAccessTokenHeader = CreateUserAccessTokenHeader();

            AddPermissionInRole(userAccessTokenHeader, _permission);

            ChangePermissionInRoleAndVerifyAccessTokenPermissionsBeforeRedisCacheTimeoutTest(userAccessTokenHeader, _permission);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestMethod]
        public void ChangePermissionInRoleAndVerifyUserAccessTokenPermissionsAfterRedisCacheTimeoutTest()
        {
            //By default Authenticator_session__SessionExpireOffsetInPercent is 10% and updating to 99%, so that the redis cache will clear in 18 seconds
            var userAccessTokenHeader = CreateUserAccessTokenHeader();

            AddPermissionInRole(userAccessTokenHeader, _permission);

            ChangePermissionInRoleAndVerifyAccessTokenPermissionsAfterRedisCacheTimeoutTest(userAccessTokenHeader, _permission);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APIGetCallWithUserAccessTokenWithNoPermissionTest()
        {
            var accessTokenHeader = CreateUserAccessTokenHeader();
            APIGetCallWithAccessTokenWithNoPermission(accessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.UserAccessToken))]
        [TestMethod]
        public void StopAccessTokenAuthenticatorAppAndMakeAPICallWithUserAccessTokenTest()
        {
            var userAccessTokenHeader = CreateUserAccessTokenHeader();
            StopAccessTokenAuthenticatorAppAndMakeAPICallWithAccessTokenTest(userAccessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.ServiceIDAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APIGetCallWithServiceIDAccessTokenTest()
        {
            var serviceIDAccessTokenHeader = CreateServiceIDAccessTokenHeader();
            APIGetCallWithAccessToken(serviceIDAccessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.ServiceIDAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void CRUDApiCallWithServiceIDAccessTokenTest()
        {
            var serviceIDAccessTokenHeader = CreateServiceIDAccessTokenHeader();
            CRUDApiCallWithAccessToken(serviceIDAccessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.ServiceIDAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APICallsWithMultipleServiceIDAccessTokenAndVerifySpecificAccessTokenIntrospectValueTest()
        {
            var serviceIDAccessTokenHeader1 = CreateServiceIDAccessTokenHeader();
            var serviceIDAccessTokenHeader2 = CreateServiceIDAccessTokenHeader();
            var serviceIDAccessTokenHeader3 = CreateServiceIDAccessTokenHeader();
            var serviceIDAccessTokenHeader4 = CreateServiceIDAccessTokenHeader();
            var serviceIDAccessTokenHeader5 = CreateServiceIDAccessTokenHeader();

            APICallsWithMultipleAccessTokenAndVerifySpecificAccessTokenIntrospectValue(serviceIDAccessTokenHeader1, serviceIDAccessTokenHeader2, serviceIDAccessTokenHeader3, serviceIDAccessTokenHeader4, serviceIDAccessTokenHeader5);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.ServiceIDAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void ChangePermissionInRoleAndVerifyServiceIDAccessTokenPermissionsBeforeRedisCacheTimeoutTest()
        {
            var serviceIDAccessTokenHeader = CreateServiceIDAccessTokenHeader();

            AddPermissionInRole(serviceIDAccessTokenHeader, _permission);

            ChangePermissionInRoleAndVerifyAccessTokenPermissionsBeforeRedisCacheTimeoutTest(serviceIDAccessTokenHeader, _permission);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.ServiceIDAccessToken))]
        [TestMethod]
        public void ChangePermissionInRoleAndVerifyServiceIDAccessTokenPermissionsAfterRedisCacheTimeoutTest()
        {
            //By default Authenticator_session__SessionExpireOffsetInPercent is 10% and updating to 99%, so that the redis cache will clear in 18 seconds
            var serviceIDAccessTokenHeader = CreateServiceIDAccessTokenHeader();

            AddPermissionInRole(serviceIDAccessTokenHeader, _permission);

            ChangePermissionInRoleAndVerifyAccessTokenPermissionsAfterRedisCacheTimeoutTest(serviceIDAccessTokenHeader, _permission);
        }

        [TestCategory(nameof(TestCategory.GatedSanity))]
        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.ServiceIDAccessToken))]
        [TestCategory(nameof(TestCategory.OnPrem))]
        [TestMethod]
        public void APIGetCallWithServiceIDAccessTokenWithNoPermissionTest()
        {
            var accessTokenHeader = CreateServiceIDAccessTokenHeader();
            APIGetCallWithAccessTokenWithNoPermission(accessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.Nightly))]
        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestCategory(nameof(TestCategory.ServiceIDAccessToken))]
        [TestMethod]
        public void StopAccessTokenAuthenticatorAppAndMakeAPICallWithServiceIDAccessTokenTest()
        {
            var serviceIDAccessTokenHeader = CreateServiceIDAccessTokenHeader();
            StopAccessTokenAuthenticatorAppAndMakeAPICallWithAccessTokenTest(serviceIDAccessTokenHeader);
        }

        [TestCategory(nameof(TestCategory.WithoutMultitenancy))]
        [TestMethod]
        public async Task HttpToHttpsRedirectionUsingAPIGatewayTest()
        {
            var apiGatewayUrl = pipelineConfigs.APIGatewayBaseUrl.Replace("https","http");
            AssertTest.IsFalse(apiGatewayUrl.Contains("https"), failMsg: "Invalid Url", passMsg: "Valid Url");
            var userAccessTokenHeader = CreateUserAccessTokenHeader();

            var response = await HttpClientUtility.ExecuteAsyncWithoutHttpRedirection(HttpMethod.Get,apiGatewayUrl+appConfigs.LessPayloadMockservice, userAccessTokenHeader,null);
            Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);

            var redirectedUrl = response.Headers.Location.AbsoluteUri;
            var redirectedResponse = HttpClientUtility.ExecuteAndGetResponse(response.RequestMessage.Method, redirectedUrl, userAccessTokenHeader, null);
            AssertTest.IsTrue(redirectedResponse != null && redirectedResponse["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received success response");

        }


        #region Private methods

        private void APIGetCallWithAccessToken(Dictionary<string, string> accessTokenHeader)
        {
            Report.Step(@"API call with access token", @"Should get the valid upstream response");

            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, accessTokenHeader, null);
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received success response");
        }

        private void CRUDApiCallWithAccessToken(Dictionary<string, string> accessTokenHeader)
        {
            Report.Step(@"GET API call with access token", @"Should get the valid upstream response");
            var getResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, accessTokenHeader, null);
            AssertTest.IsTrue(getResponseBody != null && getResponseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received success response");


            Report.Step(@"POST API call with access token", @"Should post successfully");
            var content = HttpClientUtility.CreateHttpContent(getResponseBody.ToString());
            var postResponse = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Post, defaultEndpointUrl, accessTokenHeader, content);
            AssertTest.IsTrue(postResponse != null && postResponse["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received success response");


            Report.Step(@"PUT API call with access token", @"Should update successfully");
            string _heavyPayloadMockServiceUrl = $"{pipelineConfigs.APIGatewayBaseUrl}{ appConfigs.HeavyPayloadMockservice}";
            getResponseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, _heavyPayloadMockServiceUrl, accessTokenHeader, null);
            content = HttpClientUtility.CreateHttpContent(getResponseBody.ToString());
            var putResponse = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Put, defaultEndpointUrl, accessTokenHeader, content);
            AssertTest.IsTrue(putResponse != null && putResponse["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received success response");


            Report.Step(@"DELETE API call with access token", @"Should delete successfully");
            var deleteResponse = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Delete, _heavyPayloadMockServiceUrl, accessTokenHeader, null);
            AssertTest.IsTrue(deleteResponse != null && deleteResponse["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received success response");
        }

        private void ChangePermissionInRoleAndVerifyAccessTokenPermissionsBeforeRedisCacheTimeoutTest(Dictionary<string, string> accessTokenHeader, string permission)
        {
            try
            {
                APIGetCallAndRemoveAccessTokenPermissionInRole(accessTokenHeader, permission);

                List<JToken> permissionsFromIntrospectValue = APICallAndGetPermissionsFromIntrospectValue(accessTokenHeader);

                Report.Step(@"Access token permissions should not change before redis cache timeout",
                  @"The permissions should not be changed");
                AssertTest.IsTrue(permissionsFromIntrospectValue.Contains(permission), failMsg: $"Permission: {permission} removed is updated in redis cache", passMsg: $"Permission: {permission} removed is not updated in redis cache");
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
            finally
            {
                AddPermissionInRole(accessTokenHeader, permission);
            }
        }

        private void ChangePermissionInRoleAndVerifyAccessTokenPermissionsAfterRedisCacheTimeoutTest(Dictionary<string, string> userAccessTokenHeader, string permission)
        {
            Report.Step($"Create/Update the Environment variable Authenticator_session__SessionExpireOffsetInPercent={pipelineConfigs.AuthenticatorSessionExpireOffsetInPercent}, for access token authenticator application in CF",
                            @"Environment variable should get created/updated");

            var response = _cfUtility.UpdateEnvironmentVariablesToCFApp(pipelineConfigs.CFOrgName, pipelineConfigs.CFSpaceName, pipelineConfigs.CFAuthenticatorAppName,
                new Dictionary<string, string> { { Constants.AuthenticatorSessionExpireOffsetInPercent, pipelineConfigs.AuthenticatorSessionExpireOffsetInPercent } });
            AssertTest.IsTrue(response[Constants.AuthenticatorSessionExpireOffsetInPercent] == pipelineConfigs.AuthenticatorSessionExpireOffsetInPercent, failMsg: "Env variable create/update failed", passMsg: "Env variable created/updated successfully");


            try
            {
                APIGetCallAndRemoveAccessTokenPermissionInRole(userAccessTokenHeader, permission);

                //IAM Access Token timeout 30min (1800 seconds)
                int cacheClearWaitTimeInSeconds = (int)Math.Round(1800 - (1800 * (Convert.ToDouble(pipelineConfigs.AuthenticatorSessionExpireOffsetInPercent) / 100)));
                cacheClearWaitTimeInSeconds += 10; //(10 seconds buffer)
                Report.Step($"Wait for { cacheClearWaitTimeInSeconds} seconds to get Redis cache clear", "");
                Sleep.Seconds(cacheClearWaitTimeInSeconds);

                var permissionsFromIntrospectValue = APICallAndGetPermissionsFromIntrospectValue(userAccessTokenHeader);
                Report.Step(@"Access token permissions should change after redis cache timeout",
                     @"The permissions should be changed");
                AssertTest.IsTrue(!permissionsFromIntrospectValue.Contains(permission), failMsg: $"Permission: {permission} removed, is not updated in redis cache", passMsg: $"Permission: { permission} removed, is updated in redis cache");
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
            finally
            {
                Report.Step("Executing the Finally Block", "Finally Block should get executed");
                AddPermissionInRole(userAccessTokenHeader, permission);
                _cfUtility.UpdateEnvironmentVariablesToCFApp(pipelineConfigs.CFOrgName, pipelineConfigs.CFSpaceName, pipelineConfigs.CFAuthenticatorAppName,
                    new Dictionary<string, string> { { Constants.AuthenticatorSessionExpireOffsetInPercent, "10" } });
            }
        }

        private void StopAccessTokenAuthenticatorAppAndMakeAPICallWithAccessTokenTest(Dictionary<string, string> userAccessTokenHeader)
        {
            Report.Step(@"Stop the access token authenticator application", @"Access token authenticator application should be stopped");
            AssertTest.IsTrue(_cfUtility.ChangingCFAppState(pipelineConfigs.CFOrgName, pipelineConfigs.CFSpaceName, pipelineConfigs.CFAuthenticatorAppName, AppState.stop), failMsg: "Failed to stop application", passMsg: "Application stopped successfully");

            try
            {
                Report.Step(@"API call with access token", @"Should get the 403 forbidden response");
                var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, defaultEndpointUrl, userAccessTokenHeader, null).Result;
                AssertTest.IsTrue(response.StatusCode == HttpStatusCode.Forbidden, failMsg: $"Failed to get 403 Forbidden status code, Actual: {response.StatusCode}", passMsg: "Recieved 403 Forbidden status code");
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
            finally
            {
                Report.Step(@"Start the access token authenticator application", @"Access token authenticator application should be started");
                AssertTest.IsTrue(_cfUtility.ChangingCFAppState(pipelineConfigs.CFOrgName, pipelineConfigs.CFSpaceName, pipelineConfigs.CFAuthenticatorAppName, AppState.start), failMsg: "Failed to start application", passMsg: "Application started successfully");
            }
        }

        private List<JToken> APICallAndGetPermissionsFromIntrospectValue(Dictionary<string, string> userAccessTokenHeader)
        {
            Report.Step(@"Another API call with the same access token", @"Should get the valid upstream response");
            string customUniqueRequestHeaderValue = Guid.NewGuid().ToString();
            userAccessTokenHeader.Add(Constants.CustomUniqueRequestHeaderName, customUniqueRequestHeaderValue);
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, userAccessTokenHeader, null);
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response received", passMsg: "Received success response");


            Report.Step(@"Get the recorded upstream call request from mountebank mockservice",
               @"edisp-introspect-value should be available in the upstream request headers");
            CommonBL commonBL = new CommonBL();
            var getResponseBody = commonBL.GetImposterRecordedRequests(pipelineConfigs.APIGatewayBaseUrl, userAccessTokenHeader, "LessPayloadMockservice.json");
            var headers = getResponseBody["requests"].Where(x => x["headers"][Constants.CustomUniqueRequestHeaderName] != null && x["headers"][Constants.CustomUniqueRequestHeaderName].ToString() == customUniqueRequestHeaderValue).FirstOrDefault();
            var introspectEncodedValue = headers["headers"]["edisp-introspect-value"].ToString();
            AssertTest.IsTrue(!string.IsNullOrWhiteSpace(introspectEncodedValue), failMsg: "edisp-introspect-value is not found in upstream request headers", passMsg: "edisp-introspect-value is available in upstream request headers");


            byte[] data = Convert.FromBase64String(introspectEncodedValue);
            string decodedString = Encoding.UTF8.GetString(data);
            var permissionsFromIntrospectValue = JObject.Parse(decodedString)["organizations"]["organizationList"].Where(x => x["organizationId"].ToString() == tenantDetails[Constants.AccessTokenTenantKey].IamOrganizationId).FirstOrDefault()["permissions"].ToList();
            return permissionsFromIntrospectValue;
        }

        private void APIGetCallAndRemoveAccessTokenPermissionInRole(Dictionary<string, string> accessTokenHeader, string permission)
        {
            Report.Step(@"API call with access token", @"Should get the valid upstream response");
            if (accessTokenHeader.ContainsKey("api-version"))
                accessTokenHeader.Remove("api-version");
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, defaultEndpointUrl, accessTokenHeader, null);
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: $"No Success response received, Response Body: '{responseBody}'", passMsg: "Received success response");

            RemovePermissionInRole(accessTokenHeader, permission);
        }

        private bool RemovePermissionInRole(Dictionary<string, string> accessTokenHeader, string permission)
        {
            Report.Step(@"Capture persmissions from Role in IAM for the access token", @"Should get all the permissions captured from Role in IAM");
            string roleId = _iamBL.GetRoleId(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, tenantDetails[Constants.AccessTokenTenantKey].IamOrganizationId, pipelineConfigs.AccessTokenTestRoleName);
            List<string> capturedAllPermissions = _iamBL.GetAllPersmissionsFromRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId);
            AssertTest.IsTrue(capturedAllPermissions.Count > 0, failMsg: "No permission catpured from role", passMsg: "Captured all persmission from role");


            if (capturedAllPermissions.Contains(permission))
            {
                Report.Step(@"Remove permissions in Role", @"Permissions should be removed in the Role");
                _iamBL.RemovePermissionFromRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId, _permissionsJson);
                Logger.Info($"Remove Permission: {permission}");
                bool isPermissionRemoved = !_iamBL.GetAllPersmissionsFromRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId).Contains(permission);
                AssertTest.IsTrue(isPermissionRemoved, failMsg: $"Permission: {permission} not removed", passMsg: $"Permission: {permission} removed successfully");
                return isPermissionRemoved;
            }
            else
            {
                AssertTest.IsTrue(!capturedAllPermissions.Contains(permission), failMsg: $"Permission: {permission} exist ", passMsg: $"Permission: {permission} already not exists");
                return true;
            }
        }

        private bool AddPermissionInRole(Dictionary<string, string> accessTokenHeader, string permission)
        {
            Report.Step(@"Capture persmissions from Role in IAM for the access token", @"Should get all the permissions captured from Role in IAM");

            string roleId = _iamBL.GetRoleId(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, tenantDetails[Constants.AccessTokenTenantKey].IamOrganizationId, pipelineConfigs.AccessTokenTestRoleName);
            List<string> capturedAllPermissions = _iamBL.GetAllPersmissionsFromRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId);
            AssertTest.IsTrue(capturedAllPermissions.Count > 0, failMsg: "No permission catpured from role", passMsg: "Captured all persmission from role");


            Report.Step(@"Add permissions in Role", @"Permissions should be added in the Role");
            if (!capturedAllPermissions.Contains(permission))
            {
                _iamBL.AssignPermissionToRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId, _permissionsJson);
                Logger.Info($"Assigned Permission: {permission}");
                bool isPermssionAdded = _iamBL.GetAllPersmissionsFromRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId).Contains(permission);
                AssertTest.IsTrue(isPermssionAdded, failMsg: $"Permission: {permission} not added", passMsg: $"Permission: {permission} added successfully");
                return isPermssionAdded;
            }
            else
            {
                AssertTest.IsTrue(capturedAllPermissions.Contains(permission), failMsg: $"Permission: {permission} not exist", passMsg: $"Permission: {permission} already exists");
                return true;
            }
        }

        private Dictionary<string, string> CreateUserAccessTokenHeader(string tokenType = "access_token")
        {
            var userAccessTokenHeader = HttpClientUtility.CreateUserAccessTokenHeader(pipelineConfigs,tokenType);
            AssertTest.IsTrue(userAccessTokenHeader != null, failMsg: "User Access Token header is null", passMsg: $"User Access Token header is not null");
            return userAccessTokenHeader;
        }

        private Dictionary<string, string> CreateServiceIDAccessTokenHeader()
        {
            var serviceIDAccessTokenHeader = HttpClientUtility.CreateServiceIdAccessTokenHeader(pipelineConfigs);
            AssertTest.IsTrue(serviceIDAccessTokenHeader != null, failMsg: "ServiceID Access Token header is null", passMsg: $"ServiceID Access Token header is not null:{serviceIDAccessTokenHeader}");
            return serviceIDAccessTokenHeader;
        }

        private void APIGetCallWithAccessTokenWithNoPermission(Dictionary<string, string> accessTokenHeader)
        {
            Report.Step(@"Check and remove permissions in Role if exists", @"Permissions should be remove if exist in the Role");
            HSPIAMBusinessLayer iamBL = new HSPIAMBusinessLayer();
            string roleId = iamBL.GetRoleId(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, tenantDetails[Constants.AccessTokenTenantKey].IamOrganizationId, pipelineConfigs.AccessTokenTestRoleName);
            var allPermissions = iamBL.GetAllPersmissionsFromRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId);
            if (allPermissions.Contains(_permission))
            {
                bool permissionRemovedStatus = iamBL.RemovePermissionFromRole(pipelineConfigs.IDMClientBaseUrl, accessTokenHeader, roleId, _permissionsJson);
                AssertTest.IsTrue(permissionRemovedStatus, failMsg: "Failed to removed the permission {_permission} in role", passMsg: $"Removed permission {_permission} in role");
            }


            Report.Step(@"API call with access token", @"Should get the 403 Forbidden response");
            string checkPermissionMockserviceUrl = $"{defaultEndpointUrl}/checkpermission";
            accessTokenHeader.Add("edisp-org-id", tenantDetails[Constants.AccessTokenTenantKey].IamOrganizationId);
            accessTokenHeader.Add("permission-name", _permission);
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Get, checkPermissionMockserviceUrl, accessTokenHeader, null).Result;
            AssertTest.IsTrue(response.StatusCode == HttpStatusCode.Forbidden, failMsg: $"Did not receive 403 Forbidden response, Actual: {response.StatusCode}", passMsg: "Received 403 Forbidden response");
        }

        private void APICallsWithMultipleAccessTokenAndVerifySpecificAccessTokenIntrospectValue(Dictionary<string, string> accessTokenHeader1, Dictionary<string, string> accessTokenHeader2, Dictionary<string, string> accessTokenHeader3, Dictionary<string, string> userAccessTokenHeader4, Dictionary<string, string> userAccessTokenHeader5)
        {
            AddPermissionInRole(accessTokenHeader1, _permission);

            Report.Step(@"2 API calls with first 2 different access token", @"Should get the valid upstream response for all 2 api calls");
            APIGetCallWithAccessToken(accessTokenHeader1);
            APIGetCallWithAccessToken(accessTokenHeader2);


            Report.Step(@"Remove permission with the access token", @"Permissions should be removed in Role");
            RemovePermissionInRole(accessTokenHeader3, _permission);

            Sleep.Seconds(3);
            Report.Step(@"API call with 3rd access token", @"Should get the valid upstream response for the 3rd api call");
            APIGetCallWithAccessToken(accessTokenHeader3);


            Report.Step(@"Revert the permission which earlier did with access token", @"Permissions should be reverted in Role");
            RemovePermissionInRole(accessTokenHeader3, _permission);


            Report.Step(@"API calls with last 2 access token", @"Should get the valid upstream response for the last 2 api calls");
            APIGetCallWithAccessToken(userAccessTokenHeader4);
            APIGetCallWithAccessToken(userAccessTokenHeader5);

            var introspectValuePermissions = APICallAndGetPermissionsFromIntrospectValue(accessTokenHeader3);

            Report.Step(@"Should get the correct introspect value from Redis cache for the given access token",
              @"Should get the permission removed introspect value from Redis");
            AssertTest.IsTrue(!introspectValuePermissions.Contains(_permission), failMsg: "Failed to get the correct introspect value from Reids", passMsg: "Got the correct introspect value from Reids");
        }

        #endregion
    }
}
