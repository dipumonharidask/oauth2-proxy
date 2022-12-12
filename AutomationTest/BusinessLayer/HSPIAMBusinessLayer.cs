using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Utilities;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Utilities.Wait;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer
{
    public class HSPIAMBusinessLayer
    {
        public string GetRoleId(string idmClientBaseUrl, Dictionary<string, string> headers, string OrgId, string roleName)
        {
            try
            {
                string idmClientRoleUrl = $"{idmClientBaseUrl}/Role?organizationId={OrgId}&name={roleName}";
                Logger.Info($"Get Role url: {idmClientRoleUrl}");
                headers.TryAdd("api-version", "1");
                var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, idmClientRoleUrl, headers, null);
                var entry = responseBody["entry"]?.FirstOrDefault();
                var roleId = entry?["id"]?.ToString();
                Logger.Info($"Role Id: {roleId}");
                return roleId;
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public List<string> GetAllPersmissionsFromRole(string idmClientBaseUrl, Dictionary<string, string> headers, string roleID)
        {
            try
            {
                string idmClientPermissionsUrl = $"{idmClientBaseUrl}/Permission?roleId={roleID}";
                Logger.Info($"Get Permission url: {idmClientPermissionsUrl}");
                var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, idmClientPermissionsUrl, headers, null);
                return responseBody["entry"]?.Select(x => x["name"].ToString()).ToList();
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public bool AssignPermissionToRole(string idmClientBaseUrl, Dictionary<string, string> headers, string roleID, string permissionsJson)
        {
            try
            {
                return ManipulatePersmissionsInRole(idmClientBaseUrl, "assign-permission", headers, roleID, permissionsJson);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public bool RemovePermissionFromRole(string idmClientBaseUrl, Dictionary<string, string> headers, string roleID, string permissionsJson)
        {
            try
            {
                return ManipulatePersmissionsInRole(idmClientBaseUrl, "remove-permission", headers, roleID, permissionsJson);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        private bool ManipulatePersmissionsInRole(string idmClientBaseUrl, string permissionType, Dictionary<string, string> headers, string roleID, string permissionsJson)
        {
            string iamClientRoleUrl = $"{idmClientBaseUrl}/Role/{roleID}/${permissionType}";
            Logger.Info($"Role manipulation url: {iamClientRoleUrl}");
            var httpContent = HttpClientUtility.CreateHttpContent(permissionsJson);
            var response = HttpClientUtility.ExecuteAsync(HttpMethod.Post, iamClientRoleUrl, headers, httpContent).Result;
            Sleep.Seconds(5);
            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Getting the user details from IAM
        /// </summary>
        /// <param name="iamGetUserUrl">URL to get the user details form IAM</param>
        /// <param name="userMailId">user mail id to get details</param>
        /// <param name="headers">headers</param>
        /// <returns>List of usersUUID based on mail id</returns>
        public List<string> GetUserDetails(string iamGetUserUrl, string userMailId, Dictionary<string, string> headers)
        {
            string finalUrl = $"{iamGetUserUrl}{userMailId}";
            Logger.Info($"Get Users url: {finalUrl}");
            headers.TryAdd("api-version","2");
            var responseBody = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, finalUrl, headers, null);
            if (responseBody != null)
            {
                List <string> userLoginId = responseBody["entry"]?.Where(x => x["id"] != null).Select(x => x["id"].ToString()).ToList();
                return userLoginId;
            }
            return null;
        }

        /// <summary>
        /// Deleting the user based on userName and respective Organization ID
        /// </summary>
        /// <param name="iamGetUserUrl">URL to get the user details form IAM</param>
        /// <param name="idmClientBaseUrl">IDM client Base URL</param>
        /// <param name="userMailId">user mail id to be deleted</param>
        /// <param name="headers">headers</param>
        /// <returns>HttpMessage</returns>
        public HttpResponseMessage DeleteUser(string iamGetUserUrl, string idmClientBaseUrl, string userMailId, Dictionary<string, string> headers)
        {
            List<string> userIdtoDelete = GetUserDetails(iamGetUserUrl, userMailId.ToLower(), headers);
            headers.TryAdd("api-version", "2");
            headers.TryAdd("Accept", "application/json");
            if (userIdtoDelete.Count >= 1)
            {
                string deleteUrl = $"{idmClientBaseUrl}/User/{userIdtoDelete[0]}";
                return HttpClientUtility.ExecuteAsync(HttpMethod.Delete, deleteUrl, headers, null).Result;
            }
            return null;
        }
    }
}
