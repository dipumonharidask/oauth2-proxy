using Newtonsoft.Json;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.CFUtilites.Models
{

    public class CFTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

}
