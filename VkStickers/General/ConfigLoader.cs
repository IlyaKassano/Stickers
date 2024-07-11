using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VkStickers.General
{
    internal class ConfigLoader
    {
        const string ConfigPath = "config.json";

        internal static Config GetConfig()
        {
            if (!File.Exists(ConfigPath))
                throw new FileNotFoundException(ConfigPath);

            Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath)) 
                ?? throw new NullReferenceException("Decoded config is null");

            return config;
        }
    }
}
