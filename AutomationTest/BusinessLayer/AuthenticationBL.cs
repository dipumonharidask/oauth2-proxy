using System;
using Utilities;
using Driver.UI.Common;
using Driver.UI.Interfaces;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer
{
    public class AuthenticationBL
    {
        private readonly IWebDriverUi _webDriver;

        public AuthenticationBL(IWebDriverUi webDriver)
        {
            _webDriver = webDriver;
        }

        public Cookies GetCookie(string cookieName)
        {
            Logger.InfoStartMethod();
            try
            {
                Logger.Info($"Get the cookie Named: {cookieName}");
                var cookies = _webDriver.GetAllCookies();
                return cookies.Find(x => x.CookieName.EqualsWithIgnoreCase(cookieName));
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public void SetCookie(Cookies cookie)
        {
            Logger.InfoStartMethod();
            try
            {
                Logger.Info($"Cookie Name: {cookie.CookieName}");
                Logger.Info($"Cookie Value: {cookie.CookieValue}");
                _webDriver.DeleteCookieNamed(cookie.CookieName);
                _webDriver.AddNewCookie(cookie);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }
    }
}
