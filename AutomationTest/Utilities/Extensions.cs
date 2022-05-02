using Driver.UI.Interfaces;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Tests.Common;
using System;
using Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities
{
    public static class Extensions
    {
        public static bool EqualsWithIgnoreCase(this string actual, string expected, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            Logger.InfoStartMethod();
            try
            {
                return actual.Equals(expected, stringComparison);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public static bool StartsWithIgnoreCase(this string actual, string expected, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            Logger.InfoStartMethod();
            try
            {
                return actual.StartsWith(expected, stringComparison);
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }

        public static string TakeScreenshot(this IWebDriverUi webDriver)
        {
            try
            {
                string evidenceFolderPath = BaseTest.Report.GetEvidencePath();
                string newEvidenceFolderName = BaseTest.Report.GetNewEvidenceFolderName();
                string newEvidenceFilePath = string.Format("{0}/{1}.png", evidenceFolderPath, newEvidenceFolderName);
                webDriver.TakesScreenShot(newEvidenceFilePath);
                return newEvidenceFilePath;
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }
    }
}

