using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Philips.EDI.Foundation.APIGateway.AutomationTest.Models;
using Utilities;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Utilities
{
    public class Settings
    {
        private const string _envConfigFile = "Env.json";

        private static readonly ConfigurationReader _envConfigReader = new ConfigurationReader(
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _envConfigFile));

        private static readonly string _executionEnvironment = _envConfigReader.Configuration.GetSection("ExecutionEnvironment").Value;
        private static ConfigurationReader _environmentVairableConfigReader;

        public static T GetConfiguration<T>(string configSectionName) where T : new()
        {
            try
            {
                Logger.Info($"Config section name: {configSectionName}");
                T config = new T();

                if (configSectionName.StartsWith("AppConfiguration", StringComparison.InvariantCultureIgnoreCase) || _executionEnvironment.Equals(nameof(ExecutionEnvironment.Local), StringComparison.InvariantCultureIgnoreCase))
                {
                    _envConfigReader.Configuration.GetSection(configSectionName).Bind(config);
                }
                else if (_executionEnvironment.Equals(nameof(ExecutionEnvironment.Production), StringComparison.InvariantCultureIgnoreCase))
                {
                    _environmentVairableConfigReader = new ConfigurationReader();
                    _environmentVairableConfigReader.Configuration.Bind(config);
                }
                return config;
            }
            catch (Exception ex)
            {
                Logger.InfoFailedWithException(ex);
                throw;
            }
        }
    }
}
