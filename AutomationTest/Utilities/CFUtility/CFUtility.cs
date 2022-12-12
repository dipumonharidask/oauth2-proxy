using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Philips.EDI.Foundation.APIGateway.AutomationTest.CFUtilites.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Utilities;
using Utilities.Wait;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.CFUtilites
{
    public class CFUtility
    {
        private readonly string _cfBaseUrl;
        private readonly string _cfUserName;
        private readonly string _cfPassword;
        private readonly string _cfAccessTokenUrl;


        public CFUtility(string cfBaseUrl, string cfUserName, string cfPassword, string cfAccessTokenUrl)
        {
            _cfBaseUrl = cfBaseUrl;
            _cfUserName = cfUserName;
            _cfPassword = cfPassword;
            _cfAccessTokenUrl = cfAccessTokenUrl;
        }

        public bool ChangingCFAppState(string orgName, string spaceName, string appName, AppState appState)
        {
            try
            {
                string appGuid = GetCFAppGuid(orgName, spaceName, appName);
                string postUrl = $"{_cfBaseUrl}/apps/{appGuid }/actions/{appState.ToString()}";
                Logger.Info($"CF app state Url: { postUrl}");
                using (var apiResponse = HttpClientUtility.ExecuteAsync(HttpMethod.Post, postUrl, CFRequestHeadersWithAuth(), null).Result)
                {
                    if (!apiResponse.IsSuccessStatusCode || apiResponse.Content == null)
                    {
                        Logger.Error("Response Code :" + apiResponse.StatusCode.ToString());
                        Logger.Error($"Failed to change the state of the service to {appState.ToString()} response for : {postUrl}");
                        return false;
                    }
                    bool appCurrentStatus = false;
                    if (appState == AppState.start || appState == AppState.restart)
                    {
                        appCurrentStatus = WaitUtils.WaitUntil(() => GetCFAppStatus(appGuid).EqualsWithIgnoreCase("STARTED"), CFConstants.AppStateChangeTimeoutInSeconds, CFConstants.AppStateChangeCheckFequencyInMilliSeconds);
                    }
                    else if (appState == AppState.stop)
                    {
                        appCurrentStatus = WaitUtils.WaitUntil(() => GetCFAppStatus(appGuid).EqualsWithIgnoreCase("STOPPED"), CFConstants.AppStateChangeTimeoutInSeconds, CFConstants.AppStateChangeCheckFequencyInMilliSeconds);
                    }
                    Sleep.Seconds(10);
                    return appCurrentStatus;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }

        public Dictionary<string, string> GetEnvironmentVariablesFromCFApp(string orgName, string spaceName, string appName)
        {
            try
            {
                string appGuid = GetCFAppGuid(orgName, spaceName, appName);
                string getUrl = $"{_cfBaseUrl}/apps/{appGuid}/environment_variables";
                Logger.Info($"CF Get env url: {getUrl}");
                using (HttpResponseMessage apiResponse = HttpClientUtility.ExecuteAsync(HttpMethod.Get, getUrl, CFRequestHeadersWithAuth(), null).Result)
                {
                    if (!apiResponse.IsSuccessStatusCode || apiResponse.Content == null)
                    {
                        Logger.Error("Response Code :" + apiResponse.StatusCode.ToString());
                        Logger.Error($"Failed toget env variables of the application: {appName}");

                        return null;
                    }
                    return GetEnvironmentVariablesFromResponse(apiResponse);

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return null;
            }
        }
        public Dictionary<string, string> UpdateEnvironmentVariablesToCFApp(string orgName, string spaceName, string appName, Dictionary<string, string> envVariables)
        {
            try
            {
                string appGuid = GetCFAppGuid(orgName, spaceName, appName);
                string updateUrl = $"{_cfBaseUrl}/apps/{appGuid}/environment_variables";
                Logger.Info($"CF update env url: {updateUrl}");
                var envVariablesContent = new CFEnvironmentVariable()
                {
                    EnvironmentVariables = envVariables
                };

                string json = JsonConvert.SerializeObject(envVariablesContent);

                using (HttpResponseMessage apiResponse = PatchEnvironmentVariablesToApp(updateUrl, json))
                {
                    if (!apiResponse.IsSuccessStatusCode || apiResponse.Content == null)
                    {
                        Logger.Error("Response Code :" + apiResponse.StatusCode.ToString());
                        Logger.Error($"Failed to update env variables for the application: {appName}");
                        return null;
                    }
                    ChangingCFAppState(orgName, spaceName, appName, AppState.restart);
                    return GetEnvironmentVariablesFromResponse(apiResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return null;
            }
        }

        #region private methods
        private string GetOrganizationGuid(string orgName)
        {
            string getOrgsUrl = $"{_cfBaseUrl}/organizations";
            Logger.Info($"Get Orgs url: {getOrgsUrl}");
            return GetResourceGuid(getOrgsUrl, orgName); ;
        }

        private string GetSpaceGuid(string orgGuid, string spaceName)
        {
            string getSpacesUrl = $"{_cfBaseUrl}/spaces?organization_guids={orgGuid}&page=2&per_page=50";
            Logger.Info($"Get spaces url: {getSpacesUrl}");
            return GetResourceGuid(getSpacesUrl, spaceName);
        }

        private string GetResourceGuid(string getUrl, string name)
        {
            var spacesList = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, getUrl, CFRequestHeadersWithAuth(), null)["resources"];
            var spaceGuid = spacesList.Where(x => x["name"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()["guid"].ToString();
            return spaceGuid;
        }

        private string GetCFAppGuid(string orgName, string spaceName, string appName)
        {
            string orgGuid = GetOrganizationGuid(orgName);
            string spaceGuid = GetSpaceGuid(orgGuid, spaceName);

            string getAppsUrl = $"{_cfBaseUrl}/apps?organization_guids={orgGuid}&space_guids={spaceGuid}&names={appName}";
            Logger.Info($"Get apps url: {getAppsUrl}");
            return GetResourceGuid(getAppsUrl, appName);
        }

        private string GetCFAppStatus(string cfAppGuid)
        {
            string postUrl = _cfBaseUrl + "/apps/" + cfAppGuid;
            var apiResponse = HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, postUrl, CFRequestHeadersWithAuth(), null);
            return apiResponse["state"].ToString();
        }

        private CFTokenResponse GetCFOauthToken()
        {
            var postData = new[]
            {
                      new KeyValuePair<string, string>("grant_type","password"),
                      new KeyValuePair<string, string>("username",_cfUserName),
                      new KeyValuePair<string, string>("password",_cfPassword)
                    };
            Logger.Info($"CF access token Url: {_cfAccessTokenUrl}");
            var token = PostFormUrlEncoded<CFTokenResponse>(_cfAccessTokenUrl, postData).Result;
            return token;
        }

        private async Task<T> PostFormUrlEncoded<T>(string url, IEnumerable<KeyValuePair<string, string>> postData) where T : class
        {
            using (var httpClient = new HttpClient())
            {
                string authInfo = "cf" + ":" + "";
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);

                using (var content = new FormUrlEncodedContent(postData))
                {
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string apiResponseString = response.Content.ReadAsStringAsync().Result;
                    var tokenResponse = JsonConvert.DeserializeObject<T>(apiResponseString);
                    return tokenResponse;
                }
            }
        }
        private Dictionary<string, string> CFRequestHeadersWithAuth()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            var authToken = GetCFOauthToken();
            if (!string.IsNullOrEmpty(authToken.AccessToken.ToString()))
            {
                headers.Add("Authorization", "Bearer " + authToken.AccessToken);
            }
            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");
            headers.Add("api-version", "1");
            return headers;
        }

        private static Dictionary<string, string> GetEnvironmentVariablesFromResponse(HttpResponseMessage apiResponse)
        {
            var envVariables = JObject.Parse(apiResponse.Content.ReadAsStringAsync().Result)["var"].ToString();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(envVariables);
        }

        private HttpResponseMessage PatchEnvironmentVariablesToApp(string url, string content)
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            var authToken = GetCFOauthToken();
            if (!string.IsNullOrEmpty(authToken.AccessToken.ToString()))
            {
                request.Headers.Add("Authorization", "Bearer " + authToken.AccessToken);
            }
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("api-version", "1");
            return httpClient.SendAsync(request).Result;
        }
        #endregion
    }
}
