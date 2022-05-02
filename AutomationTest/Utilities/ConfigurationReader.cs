using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities
{
    public class ConfigurationReader
    {
        public IConfiguration Configuration { get; }

        public ConfigurationReader(string jsonFilePath)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(jsonFilePath);
            Configuration = builder.Build();
        }

        public ConfigurationReader()
        {
            var builder = new ConfigurationBuilder();
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();            
        }
    }
}
