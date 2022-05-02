using Newtonsoft.Json;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Models
{
    public class OpenIdConfigOrganizationCertificateMapping
    {
        [JsonProperty("organizationId")]
        public string OrganizationId { get; set; }

        [JsonProperty("certificate")]
        public string Certificate { get; set; }
    }
}
