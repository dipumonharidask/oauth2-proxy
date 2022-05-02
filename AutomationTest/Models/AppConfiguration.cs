namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Models
{
    public class AppConfiguration
    {
        public string RootFolder { get; set; }
        public string ReporterList { get; set; }
        public string RootEvidencePath { get; set; }
        public string DifidoFolderLocation { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string LessPayloadMockservice { get; set; }
        public string HeavyPayloadMockservice { get; set; }
        public string ServiceUnavailableMockservice { get; set; }
        public string MultitenancyMockservice { get; set; }
        public string LogoutPath { get; set; }
        public string CDRSubscriptionUrlPath { get; set; }
        public string QidoStudyLevelUrlPathWithoutOrgId { get; set; }
        public string CDRImagingStudyUrlPath { get; set; }
        public string IamBrokerConfigAPIRelativePath { get; set; }
        public string OpenIdConfigurationUrlPath { get; set; }
        public int MountibankTimeoutinSeconds { get; set; }
    }
}
