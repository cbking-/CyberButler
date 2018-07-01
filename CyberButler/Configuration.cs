using Microsoft.Extensions.Configuration;
using System.IO;

namespace CyberButler
{
    internal class Configuration
    {
        public static IConfigurationRoot Config { get; private set; }

        static Configuration()
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}