using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.APITest
{
    [TestClass]
    public class PostDeploymentTests : BaseTest
    {
        [TestCategory(nameof(TestCategory.PostDeployment))]
        [TestMethod]
        public void PostDeploymentTest()
        {
            Report.Step(@"Create the mockservices with mountebank", @"Should create the new mockservices");
            bool isLoginSuccessful = LoginToEndpoint(pipelineConfigs.APIGatewayBaseUrl, pipelineConfigs.AuthUserName, pipelineConfigs.AuthPassword);
            AssertTest.IsTrue(isLoginSuccessful, failMsg: "Login Unsuccessful", passMsg: "Login Successful");
            AuthenticationBL authenticationBL = new AuthenticationBL(WebDriver);
            var cookie = authenticationBL.GetCookie(pipelineConfigs.CookieName);
            string[] mockJsonFiles = Directory.GetFiles(Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tests", "Data", "MockServiceConfigs"), "*.json");

            if (mockJsonFiles == null || mockJsonFiles.Length <= 0)
            {
                AssertTest.IsTrue(false, "No mock json files available");
            }

            foreach (var mockJsonFile in mockJsonFiles)
            {
                string mockJsonString = File.ReadAllText(mockJsonFile);
                var impostersUrl = $"{pipelineConfigs.APIGatewayBaseUrl}/imposters";
                string port = JObject.Parse(mockJsonString)["port"].ToString();
                Logger.Info($"Mockservice File: {mockJsonFile}, Port: {port}");

                var httpContent = HttpClientUtility.CreateHttpContent(mockJsonString);
                var headers = HttpClientUtility.CreateCookieHeader(pipelineConfigs.CookieName, cookie.CookieValue);
                string deleteMockServiceApiUrl = $"{ impostersUrl }/{ port}";
                Logger.Info($"Delete mockservice Url: {deleteMockServiceApiUrl}");
                var deleteResponse = HttpClientUtility.ExecuteAsync(HttpMethod.Delete, deleteMockServiceApiUrl, headers, httpContent).Result;
                AssertTest.IsTrue(deleteResponse.IsSuccessStatusCode || deleteResponse.StatusCode == HttpStatusCode.NotFound, failMsg: "Failed to delete the mockservice imposter", passMsg: "Deleted the mockservice imposter");
                Logger.Info($"Create mockservice url: {impostersUrl}");
                var postCallResponse = HttpClientUtility.ExecuteAsync(HttpMethod.Post, impostersUrl, headers, httpContent).Result;
                AssertTest.IsTrue(postCallResponse.IsSuccessStatusCode, failMsg: "Failed to create mockservice", passMsg: "Created mockservice successfully");
            }
        }
    }
}
