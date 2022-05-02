using Newtonsoft.Json;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Models
{
    public class TenantMapping
    {
        [JsonProperty("tenantName")]
        public string TenantName { get; set; }
        [JsonProperty("iamOrganizationId")]
        public string IamOrganizationId { get; set; }
    }
}
