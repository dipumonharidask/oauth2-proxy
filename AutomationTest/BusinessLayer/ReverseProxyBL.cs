using Driver.UI.Interfaces;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer
{
    public class ReverseProxyBL
    {
        private readonly IWebDriverUi _webDriver;

        public ReverseProxyBL(IWebDriverUi webDriver)
        {
            _webDriver = webDriver;
        }

        public bool NavigateToImpostersPageAndGetIsMockServiceLinksDisplayed(string impostersUrl)
        {
            _webDriver.Goto(impostersUrl);
            _webDriver.TakeScreenshot();
            return _webDriver.IsDisplayed(_webDriver.FindElementByXPath("//table[@id='imposters']"));
        }
        public bool ClickOnImposterAndGetMockServiceContentIsDisplayed(string mockserviceName)
        {
            _webDriver.Click(_webDriver.FindElementByXPath($"//a[normalize-space(.)='{mockserviceName}']"));
            _webDriver.TakeScreenshot();
            return _webDriver.IsDisplayed(_webDriver.FindElementByXPath("//code[contains(.,'predicates')]"));
        }

    }
}
