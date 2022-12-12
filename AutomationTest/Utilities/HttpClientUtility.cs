
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Driver.Api.HttpClientApi;
using Newtonsoft.Json.Linq;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Utilities;
using Utilities.Common;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities
{
    public class HttpClientUtility
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly HttpClient _httpClientWithoutRedirection = new HttpClient( new HttpClientHandler { AllowAutoRedirect = false });
        private static readonly HttpClientUtilities _httpClientUtility = new HttpClientUtilities();

        public static async Task<HttpResponseMessage> ExecuteAsync(HttpMethod httpMethod, string url, Dictionary<string, string> headers, HttpContent content)
        {
            return await ExecuteAsync(_httpClient, httpMethod, url, headers, content);
        }

        public static async Task<HttpResponseMessage> ExecuteAsyncWithoutHttpRedirection(HttpMethod httpMethod, string url, Dictionary<string, string> headers, HttpContent content)
        {
            return await ExecuteAsync(_httpClientWithoutRedirection, httpMethod, url, headers, content);
        }

        private static async Task<HttpResponseMessage> ExecuteAsync(HttpClient client,HttpMethod httpMethod, string url, Dictionary<string, string> headers, HttpContent content)
        {
            try
            {
                return (httpMethod.ToString()) switch
                {
                    "POST" => await _httpClientUtility.HttpPostAsync(client, url, headers, content),
                    "PUT" => await _httpClientUtility.HttpPutAsync(client, url, headers, content),
                    "DELETE" => await _httpClientUtility.HttpDeleteAsync(client, url, headers),
                    _ => await _httpClientUtility.HttpGetAsyncResp(client, url, headers),//GET
                };
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
            }

            return null;
        }

        public static StringContent CreateHttpContent(string content, string MediaType = "application/json")
        {
            Logger.InfoStartMethod();
            try
            {
                return new StringContent(content, Encoding.UTF8, MediaType);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public static Dictionary<string, string> CreateCookieHeader(string cookieName, string cookieValue)
        {
            Logger.InfoStartMethod();
            try
            {
                Dictionary<string, string> cookieHeader = new Dictionary<string, string>
                {
                    { "Cookie", $"{cookieName}={cookieValue}" }
                };
                return cookieHeader;
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public static Dictionary<string, string> CreateUserAccessTokenHeader(PipelineConfiguration pipelineConfiguration, string tokenType = "access_token")
        {
            Logger.InfoStartMethod();
            try
            {
                Logger.Info($"Access token url: {pipelineConfiguration.IAMAuthorizationUrl}");
                var headers = new Dictionary<string, string>();
                headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{pipelineConfiguration.OauthClientID}:{pipelineConfiguration.OauthClientSecret}"))}");
                string content = $"grant_type=password&username={pipelineConfiguration.AuthUserName}&password={pipelineConfiguration.AuthPassword}";
                var httpContent = CreateHttpContent(content, "application/x-www-form-urlencoded");
                var responseBody = ExecuteAndGetResponse(HttpMethod.Post, pipelineConfiguration.IAMAuthorizationUrl, headers, httpContent);
                string authToken = responseBody[tokenType]?.ToString();
                if(authToken == null)
                {
                    return null;
                }
                var userAccessTokenHeader = new Dictionary<string, string>();
                userAccessTokenHeader.Add("Authorization", $"Bearer {authToken}");
                return userAccessTokenHeader;
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public static Dictionary<string, string> CreateServiceIdAccessTokenHeader(PipelineConfiguration pipelineConfig)
        {
            Logger.InfoStartMethod();
            try
            {
                string serviceIDAccessToken = CommonFunctionality.GetAccessToken(pipelineConfig.ServiceID, pipelineConfig.ServiceIDPrivateKey, pipelineConfig.IAMAccessTokenUrl, pipelineConfig.IAMAuthorizationUrl);
                var serviceIDAccessTokenHeader = new Dictionary<string, string>();
                serviceIDAccessTokenHeader.Add("Authorization", $"Bearer {serviceIDAccessToken}");
                return serviceIDAccessTokenHeader;
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public static JObject ExecuteAndGetResponse(HttpMethod httpMethod, string url, Dictionary<string, string> headers, HttpContent content)
        {
            Logger.InfoStartMethod();
            try
            {
                var response = ExecuteAsync(httpMethod, url, headers, content).Result.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(response))
                {
                    return null;
                }
                return JObject.Parse(response);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }
    }
}
