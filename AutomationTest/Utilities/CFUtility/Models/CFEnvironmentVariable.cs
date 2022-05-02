using Newtonsoft.Json;
using System.Collections.Generic;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.CFUtilites.Models
{
    public class CFEnvironmentVariable
    {
        [JsonProperty("var")]
        public Dictionary<string, string> EnvironmentVariables { get; set; }
    }
}
