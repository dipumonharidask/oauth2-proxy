using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities;
using Utilities.Enums;
using Driver.UI.Interfaces;
using Driver.UI.Selenium;
using Utilities.Wait;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common
{
    [TestClass]
    public abstract class BaseTest
    {

        public static IWebDriverUi WebDriver
        {
            get => _WebDriver;
        }
        public static ReportManager Report
        {
            get => _report;
        }
        public TestContext TestContext
        {
            get => _testContext;
            set => _testContext = value;
        }

        public static readonly AppConfiguration appConfigs = Settings.GetConfiguration<AppConfiguration>(typeof(AppConfiguration).Name);
        public static readonly PipelineConfiguration pipelineConfigs = Settings.GetConfiguration<PipelineConfiguration>(typeof(PipelineConfiguration).Name);
        public static readonly Dictionary<string, TenantMapping> tenantDetails = JsonConvert.DeserializeObject<Dictionary<string, TenantMapping>>(pipelineConfigs.TenantDetails);
        public readonly string defaultEndpointUrl = $"{pipelineConfigs.APIGatewayBaseUrl}{ appConfigs.LessPayloadMockservice}";
        private ReporterBase _ReporterBase;
        private static IWebDriverUi _WebDriver;
        private static ReportManager _report;
        private TestContext _testContext;
        private readonly string _outputFolder = Path.Combine(appConfigs.RootFolder, "Logs");
        private readonly ReporterTestInfo _testInfo = new ReporterTestInfo();

        [TestInitialize]
        public void Before()
        {
            Logger.Info("######### Start Test ######### Test Name: " + _testContext.TestName + "\r\n");
            _ReporterBase = new ReporterBase(_outputFolder, appConfigs.ReporterList, appConfigs.RootFolder,
                          appConfigs.RootEvidencePath, appConfigs.DifidoFolderLocation, appConfigs.ProductName);
            _report = _ReporterBase.GetReportMngInstance(_testContext.TestName, _testContext.FullyQualifiedTestClassName, _testInfo);
            AssertTest.ConfigureServices(TestProjectType.MsTest);
            AssertTest.InitAssertService();
            ProcessUtilities.KillChromeDriver();
        }

        [TestCleanup]
        public void After()
        {
            if (!TestContext.CurrentTestOutcome.Equals(UnitTestOutcome.Passed) &&
                Report.CurrentStepStatus == Reporters.BaseReport.Enums.Enum.StepStatus.Passed)
                Report.ReportError("The test has failed by throw an exception (not by any assert validation)");

            _ReporterBase.CloseReports(_testInfo);

            if (Report != null && WebDriver != null)
            {
                _ = WebDriver.Quit();
            }
            ProcessUtilities.KillChromeDriver();
            Logger.Info("######### End Test ######### Test Name: " + _testContext.TestName + "\r\n");
        }

        protected bool LoginToEndpoint(string endpointUrl, string userName, string password)
        {
            Logger.InfoStartMethod();
            try
            {
                Logger.Info($"Endpoint url: {endpointUrl}");
                _WebDriver = new SeleniumDriver(Driver.Enums.BrowserType.Chrome, endpointUrl, null);

                object userNameElement = null;
                WaitUtils.WaitUntil(() =>
                {
                    userNameElement = WebDriver.FindElementById("idToken1");
                    return WebDriver.IsExist(userNameElement);
                }, 60);

                _ = WebDriver.TypeText(userNameElement, userName);

                _ = WebDriver.TypeText(WebDriver.FindElementById("idToken2"), password);

                _ = WebDriver.Click(WebDriver.FindElementById("loginButton_0"));

                var saveConsentCheckBox = WebDriver.FindElementById("saveConsent");

                if (WebDriver.IsExist(saveConsentCheckBox))
                {
                    Logger.Info($"Save Consent exists");

                    _ = WebDriver.Click(saveConsentCheckBox);

                    _ = WebDriver.Click(WebDriver.FindElementByXPath("//button[@value='allow']"));
                }

                var bodyElement = WebDriver.FindElementByXPath("//body/pre | //a[text()='home']");

                return WebDriver.IsExist(bodyElement);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
            finally
            {
                WebDriver.TakeScreenshot();
            }
        }

        protected JObject GetResponseFromUI()
        {
            try
            {
                var bodyElement = WebDriver.FindElementByXPath("//body/pre");
                if (WebDriver.IsExist(bodyElement))
                {
                    string responseJson = WebDriver.GetText(bodyElement);
                    return JObject.Parse(responseJson);
                }
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
            return null;
        }

        protected bool IsLoginPageDisplayed()
        {
            var userNameElement = WebDriver.FindElementById("idToken1");
            var passwordElement = WebDriver.FindElementById("idToken2");
            return WebDriver.IsExist(userNameElement) && WebDriver.IsExist(passwordElement);
        }
    }
}
