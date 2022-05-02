using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer
{
    public class CommonBL
    {

        public JObject GetImposterRecordedRequests(string apiGatewayBaseUrl, Dictionary<string, string> headers, string mockserviceJsonConfigFile)
        {
            try
            {
                string mockserviceJsonConfigFilePath = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tests", "Data", "MockServiceConfigs", mockserviceJsonConfigFile);
                Logger.Info($"Mockservice json config file path: {mockserviceJsonConfigFilePath}");
                string port = JObject.Parse(File.ReadAllText(mockserviceJsonConfigFilePath))["port"].ToString();
                string imposterGetUrl = $"{apiGatewayBaseUrl}/imposters/{port}";
                Logger.Info($"Imposter Get Url: { imposterGetUrl}");
                return HttpClientUtility.ExecuteAndGetResponse(HttpMethod.Get, imposterGetUrl, headers, null);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public JToken GetUpstreamRecordedRequest(string apiGatewayBaseUrl, Dictionary<string, string> headers, string customUniqueRequestIdHeaderName, string customUniqueRequestIdHeaderValue, string mockserviceJsonConfigFile)
        {
            try
            {
                var getResponseBody = GetImposterRecordedRequests(apiGatewayBaseUrl, headers, mockserviceJsonConfigFile);
                Logger.Info($"Response body: {getResponseBody}");
                JToken testRecordedRequest = getResponseBody["requests"].Where(x => x["headers"][customUniqueRequestIdHeaderName] != null && x["headers"][customUniqueRequestIdHeaderName].ToString() == customUniqueRequestIdHeaderValue).FirstOrDefault();
                return testRecordedRequest;
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }
    }
}
