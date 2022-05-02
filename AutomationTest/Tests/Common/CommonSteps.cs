using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;
using Reporters;
using Utilities.Wait;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common
{
    public class CommonSteps : BaseTest
    {
        public void BrowseEndpointAndVerifyLogin(string endpoint, string userName, string password)
        {
            Report.Step(@"Browse the endpoint url and login", @"Should successfully login and get the upstream response");
            var isLoginSuccessful = LoginToEndpoint(endpoint, userName, password);
            AssertTest.IsTrue(isLoginSuccessful, failMsg: "Login Unsuccessful", passMsg: "Login Successful");
            var responseBody = GetResponseFromUI();
            AssertTest.IsTrue(responseBody != null && responseBody["status"].ToString().EqualsWithIgnoreCase("success"), failMsg: "No Success response", passMsg: "Received success response");
        }

        public bool LogoutWithPOST(string endpointUrl, string userName, string password)
        {
            try
            {
                IWebDriver _localWebDriver = new ChromeDriver();
                _localWebDriver.Url = defaultEndpointUrl;
                _localWebDriver.Navigate();
                
                WebDriverWait wait = new WebDriverWait(_localWebDriver,TimeSpan.FromSeconds(20));

                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("idToken1")));

                IWebElement userNameElement = null;
                WaitUtils.WaitUntil(() =>
                {
                    userNameElement = _localWebDriver.FindElement(By.Id("idToken1"));
                    return userNameElement.Displayed;
                }, 60);

                userNameElement.SendKeys(userName);
                _localWebDriver.FindElement(By.Id("idToken2")).SendKeys(password); ;

                _localWebDriver.FindElement(By.Id("loginButton_0")).Click();

                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//body/pre | //a[text()='home']")));
                IWebElement bodyElement = _localWebDriver.FindElement(By.XPath("//body/pre | //a[text()='home']"));

                if(bodyElement.Displayed)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_localWebDriver;
                    string title = (string)js.ExecuteScript("navigator.sendBeacon('/logout');");
                }

                _localWebDriver.Navigate().Refresh();
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("idToken1")));
                userNameElement = _localWebDriver.FindElement(By.Id("idToken1"));
                bool isLoginPage = userNameElement.Enabled;
                _localWebDriver.Quit();
                return isLoginPage;
            }
            catch (Exception ex)
            {
                Report.ReportError(ex.ToString(), "LogoutWithPOST");
                throw;
            }
        }
    }

    


}
